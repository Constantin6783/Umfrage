using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PollTool.Server.Models.Requests;
using PollTool.Server.Models.Response;
using ApiPoll = PollTool.Server.Models.Api.Poll;
using DbPoll = PollTool.Server.Models.Database.Poll;
using ApiQuestion = PollTool.Server.Models.Api.Question;
using DbQuestion = PollTool.Server.Models.Database.Question;
using ApiAnswer = PollTool.Server.Models.Api.Answer;
using DbAnswer = PollTool.Server.Models.Database.Answer;
using PollTool.Server.Models.Database;
using PollTool.Server.Models.Api;

namespace PollTool.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PollsController : ControllerBase
    {
        private const string CRITICAL_ERROR = "Critical error, please contact the administrator";

        private readonly PollContext _context;
        private readonly ILogger _logger;

        public string CurrentUserId
        {
            get
            {
                if (!HttpContext.Session.Keys.Any())
                    HttpContext.Session.SetString("ID", HttpContext.Session.Id);
                return HttpContext.Session.Id;
            }
        }

        public PollsController(PollContext context, ILogger<PollsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Polls
        [HttpPost]
        public async Task<ActionResult<GetPollsResponse>> GetPolls([FromBody] BaseRequest request)
        {
            if (!request.IsValid()) return BadRequest();
            var response = new GetPollsResponse();
            try
            {
                var uid = CurrentUserId;
                var polls = await _context.Polls.Select(p => new ApiPoll
                {
                    Title = p.Title,
                    Description = p.Description,
                    PollId = p.PollId,
                    DoneByUser = _context.Questions.Where(q => q.PollId == p.PollId)
                                                   .Any(q => _context.UserAnswers.Any(ua => ua.QuestionId == q.QuestionId && ua.AnsweredBy == uid)),
                    HasAnswers = _context.Questions.Where(q => q.PollId == p.PollId)
                                                   .Any(q => _context.UserAnswers.Any(ua => ua.QuestionId == q.QuestionId)),
                    OwnedByUser = p.CreatedBy == uid
                }).ToListAsync();
                response.Polls = polls;
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(GetPolls));
                response.ErrorMessage = CRITICAL_ERROR;
            }

            return response;
        }

        // POST: api/CreatePoll
        [HttpPost]
        public async Task<ActionResult<BaseResponse>> CreatePoll([FromBody] WritePollRequest request)
        {
            if (!request.IsValid()) return BadRequest();
            var response = new BaseResponse();
            try
            {
                if (_context.Polls.Any(p => p.Title == request.Title && p.PollId != request.PollId))
                    return new BaseResponse { ErrorMessage = $"Poll '{request.Title}' already exists!" };

                if (!request.Questions.Any())
                    return new BaseResponse { ErrorMessage = $"At least one question is required!" };

                if (request.Questions.Any(q => q.Answers.Count < 2))
                    return new BaseResponse { ErrorMessage = $"At least two answers are required per question!" };

                var uid = CurrentUserId;

                DbPoll poll = null;
                if (request.PollId > 0 && _context.Polls.FirstOrDefault(p => p.PollId == request.PollId && p.CreatedBy == uid) is DbPoll tmpPoll)
                    poll = tmpPoll;
                else
                    poll = _context.Polls.Add(new DbPoll { CreatedBy = uid }).Entity;

                poll.Title = request.Title;
                poll.Description = request.Description;

                await _context.SaveChangesAsync();


                if (request.PollId > 0)
                {
                    var removableQuestions = await _context.Questions.Where(q => q.PollId == request.PollId)
                        .Select(q => new { Question = q, Answers = _context.Answers.Where(a => a.QuestionId == q.QuestionId).ToList() })
                        .ToListAsync();

                    foreach (var item in removableQuestions)
                    {
                        _context.Questions.Remove(item.Question);
                        _context.Answers.RemoveRange(item.Answers);
                    }
                    await _context.SaveChangesAsync();
                }

                foreach (var apiQuestion in request.Questions)
                {
                    var question = _context.Questions.Add(new DbQuestion
                    {
                        PollId = poll.PollId,
                        Title = apiQuestion.Title
                    }).Entity;
                    await _context.SaveChangesAsync();

                    _context.Answers.AddRange(apiQuestion.Answers.Select(a => new DbAnswer
                    {
                        QuestionId = question.QuestionId,
                        Text = a.Text
                    }));
                    await _context.SaveChangesAsync();
                }
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(CreatePoll));
                response.ErrorMessage = CRITICAL_ERROR;
            }

            return response;
        }

        // POST: api/Polls/DeletePoll
        [HttpPost]
        public async Task<ActionResult<BaseResponse>> DeletePoll([FromBody] DeletePollRequest request)
        {
            if (!request.IsValid()) return BadRequest();
            try
            {

                var uid = CurrentUserId;
                if (await _context.Polls.FindAsync(request.PollId) is DbPoll foundPoll)
                {
                    if (foundPoll.CreatedBy != uid) return Unauthorized();
                    var queryQuestions = _context.Questions.Where(q => q.PollId == request.PollId);

                    _context.Answers.RemoveRange(_context.Answers.Join(queryQuestions, a => a.QuestionId, q => q.QuestionId, (ar, aq) => ar).ToList());
                    _context.UserAnswers.RemoveRange(_context.UserAnswers.Join(queryQuestions, ua => ua.QuestionId, q => q.QuestionId, (uar, aq) => uar).ToList());
                    _context.Questions.RemoveRange(queryQuestions.ToList());
                    _context.Polls.Remove(foundPoll);
                    await _context.SaveChangesAsync();

                    return new BaseResponse { Success = true };

                }

                return new BaseResponse { ErrorMessage = "Poll not found!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(DeletePoll));
                return new BaseResponse { ErrorMessage = CRITICAL_ERROR };
            }
        }

        // POST: api/Polls/GetPoll
        [HttpPost]
        public async Task<ActionResult<GetPollResponse>> GetPoll([FromBody] GetPollRequest request)
        {
            if (!request.IsValid()) return BadRequest();
            try
            {
                var uid = CurrentUserId;

                if (await PollDoneByUser(request.PollId, uid))
                    return new GetPollResponse { Success = false, ErrorMessage = $"Already processed!" };

                var response = await _context.Polls.Where(p => p.PollId == request.PollId)
                    .Select(p => new GetPollResponse
                    {
                        Poll = new ApiPoll
                        {
                            PollId = p.PollId,
                            Description = p.Description,
                            Title = p.Title
                        },

                    }).FirstOrDefaultAsync();

                if (response == null) return new GetPollResponse { ErrorMessage = "Poll not found!" };
                response.Questions = await _context.Questions.Where(q => q.PollId == request.PollId).Select(q => new ApiQuestion
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    Answers = _context.Answers.Where(a => a.QuestionId == q.QuestionId).Select(a => new ApiAnswer
                    {
                        Text = a.Text,
                        AnswerId = a.AnswerId,
                    }).ToList()
                }).ToListAsync();

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(GetPoll));
                return new GetPollResponse { ErrorMessage = CRITICAL_ERROR };
            }
        }

        // POST: api/Polls/ProcessPoll
        [HttpPost]
        public async Task<ActionResult<BaseResponse>> ProcessPoll([FromBody] ProcessPollRequest request)
        {
            if (!request.IsValid()) return BadRequest();

            try
            {
                var uid = CurrentUserId;
                var questionIds = request.Answereds.Select(a => a.QuestionId).ToList();

                if (questionIds.Count > questionIds.Distinct().Count())
                    return new BaseResponse { ErrorMessage = $"Questions may only be answered once!" };

                if (!await _context.Polls.AnyAsync(p => p.PollId == request.PollId))
                    return new BaseResponse { ErrorMessage = $"Poll not found!" };

                if (await _context.Questions.AnyAsync(q => q.PollId == request.PollId && !questionIds.Contains(q.QuestionId)))
                    return new BaseResponse { ErrorMessage = $"Questions not matching Poll!" };

                var questionPossibleAnswers = await _context.Questions.Where(q => questionIds.Contains(q.QuestionId))
                    .Select(q => new
                    {
                        q.QuestionId,
                        PossibleAnswerIds = _context.Answers.Where(a => a.QuestionId == q.QuestionId).Select(a => a.AnswerId).ToList(),
                    }).ToListAsync();

                foreach (var questionAnswers in questionPossibleAnswers)
                {
                    if (request.Answereds.Single(a => a.QuestionId == questionAnswers.QuestionId) is AnsweredQuestion aq &&
                       !questionAnswers.PossibleAnswerIds.Contains(aq.SelectedAnswer))
                        return new BaseResponse { ErrorMessage = $"Answers not matching Questions!" };
                }

                if (await PollDoneByUser(request.PollId, uid)) return new BaseResponse { Success = false, ErrorMessage = $"Already processed!" };

                _context.UserAnswers.AddRange(request.Answereds.Where(a => questionPossibleAnswers.Any(q => q.QuestionId == a.QuestionId)).Select(a => new UserAnswer
                {
                    AnsweredBy = uid,
                    AnswerId = a.SelectedAnswer,
                    QuestionId = a.QuestionId
                }));

                await _context.SaveChangesAsync();
                return new BaseResponse { Success = true };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(ProcessPoll));
                return new BaseResponse { ErrorMessage = CRITICAL_ERROR };
            }
        }

        [HttpPost]
        public async Task<ActionResult<PollStatisticResponse>> PollStatistic([FromBody] GetPollRequest request)
        {
            if (!request.IsValid()) return BadRequest();

            try
            {
                var response = await _context.Polls.Where(p => p.PollId == request.PollId).Select(p => new PollStatisticResponse
                {
                    PollDescription = p.Description,
                    PollTitle = p.Title,
                    Questions = _context.Questions.Where(q => q.PollId == p.PollId)
                    .Select(q => new UserQuestion
                    {
                        Question = q.Title,
                        Answers = _context.Answers.Where(a => a.QuestionId == q.QuestionId).Select(a => new UserQuestionAnswer
                        {
                            Answer = a.Text,
                            Count = _context.UserAnswers.Count(ua => ua.AnswerId == a.AnswerId)
                        }).ToList()
                    }).ToList()
                }).FirstOrDefaultAsync();

                if (response != null)
                {
                    response.Success = true;
                    return response;
                }

                return new PollStatisticResponse { ErrorMessage = "Poll not found!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(PollStatistic));
                return new PollStatisticResponse { ErrorMessage = CRITICAL_ERROR };
            }
        }


        [HttpPost]
        public async Task<ActionResult<EditPollResponse>> BeginEditPoll([FromBody] GetPollRequest request)
        {
            if (!request.IsValid()) return BadRequest();

            try
            {
                var uid = CurrentUserId;

                if (await _context.UserAnswers.AnyAsync(ua => _context.Questions.Where(q => q.PollId == request.PollId).Any(q => q.QuestionId == ua.QuestionId)))
                    return new EditPollResponse { ErrorMessage = "Poll already answered, editing not possible!" };

                var response = await _context.Polls.Where(p => p.PollId == request.PollId && p.CreatedBy == uid).Select(p => new EditPollResponse
                {
                    PollId = p.PollId,
                    Description = p.Description,
                    Title = p.Title,
                    Questions = _context.Questions.Where(q => q.PollId == p.PollId)
                    .Select(q => new ApiQuestion
                    {
                        Title = q.Title,
                        Answers = _context.Answers.Where(a => a.QuestionId == q.QuestionId).Select(a => new ApiAnswer
                        {
                            AnswerId = a.AnswerId,
                            Text = a.Text
                        }).ToList()
                    }).ToList()
                }).FirstOrDefaultAsync();

                if (response != null)
                {
                    response.Success = true;
                    return response;
                }

                return new EditPollResponse { ErrorMessage = "Poll not found!" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(PollStatistic));
                return new EditPollResponse { ErrorMessage = CRITICAL_ERROR };
            }
        }

        private async Task<bool> PollDoneByUser(int pollID, string userID)
        {
            return await _context.Questions.Where(q => q.PollId == pollID)
                    .Join(_context.UserAnswers.Where(ua => ua.AnsweredBy == userID), q => q.QuestionId, ua => ua.QuestionId, (qr, uar) => uar.QuestionId)
                    .AnyAsync();
        }
    }
}
