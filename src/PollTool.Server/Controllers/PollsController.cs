using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Sqlite.Storage.Internal;
using PollTool.Server.Models.Api;
using PollTool.Server.Models.Requests;
using PollTool.Server.Models.Response;
using ApiPoll = PollTool.Server.Models.Api.Poll;
using DbPoll = PollTool.Server.Models.Database.Poll;
using ApiQuestion = PollTool.Server.Models.Api.Question;
using DbQuestion = PollTool.Server.Models.Database.Question;
using ApiAnswer = PollTool.Server.Models.Api.Answer;
using DbAnswer = PollTool.Server.Models.Database.Answer;
using PollTool.Server.Models.Database;
namespace PollTool.Server.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class PollsController : ControllerBase
    {
        private readonly PollContext _context;
        private readonly ILogger _logger;

        public long CurrentUserID => HttpContext.Connection.RemoteIpAddress.MapToIPv4().Address;

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
                //var uid = HttpContext.Connection.Id
                var uid = CurrentUserID;
                var ip = HttpContext.Connection.RemoteIpAddress.ToString();
                var polls = await _context.Polls.Select(p => new ApiPoll
                {
                    Title = p.Title,
                    Description = p.Description,
                    PollId = p.PollId,
                    DoneByUser = _context.Questions.Where(q => q.PollId == p.PollId)
                    .Any(q => _context.UserAnswers.Any(ua => ua.QuestionId == q.QuestionId && ua.UserAnswerId == uid)),
                    OwnedByUser = p.CreatedBy == ip
                }).ToListAsync();
                response.Polls = polls;
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(GetPolls));
                response.ErrorMessage = "Critical error, please contact the administrator";
            }


            return response;
        }

        // POST: api/CreatePoll
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CreatePollResponse>> CreatePoll([FromBody] CreatePollRequest request)
        {
            if (!request.IsValid()) return BadRequest();


            var response = new CreatePollResponse();


            try
            {
                if (_context.Polls.Any(p => p.Title == request.Title)) return new CreatePollResponse { ErrorMessage = $"Umfrage '{request.Title}' existiert bereits!" };
                var ip = HttpContext.Connection.RemoteIpAddress.ToString();
                var poll = _context.Polls.Add(new DbPoll
                {
                    Title = request.Title,
                    Description = request.Description,
                    CreatedBy = ip
                }).Entity;
                await _context.SaveChangesAsync();

                var question = _context.Questions.Add(new DbQuestion
                {
                    PollId = poll.PollId,
                    Title = request.Title,
                }).Entity;

                await _context.SaveChangesAsync();

                _context.Answers.AddRange(request.Answers.Select(a => new DbAnswer { QuestionId = question.QuestionId, Text = a.Text }));

                await _context.SaveChangesAsync();
                response.Success = true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(CreatePoll));
                response.ErrorMessage = "Critical error, please contact the administrator";
            }

            return response;
        }

        // POST: api/Polls/DeletePoll
        [HttpPost]
        public async Task<ActionResult<BaseResponse>> DeletePoll([FromBody] DeletePollRequest request)
        {
            if (!request.IsValid()) return BadRequest();

            var ip = HttpContext.Connection.RemoteIpAddress.ToString();
            if (await _context.Polls.FindAsync(request.PollID) is DbPoll foundPoll)
            {

                if (foundPoll.CreatedBy != ip) return Unauthorized();

                _context.Polls.Remove(foundPoll);
                await _context.SaveChangesAsync();
                return new BaseResponse { Success = true };

            }

            return new BaseResponse { ErrorMessage = "Poll not found!" };
        }


        // POST: api/Polls/GetPoll
        [HttpPost]
        public async Task<ActionResult<GetPollResponse>> GetPoll([FromBody] GetPollRequest request)
        {
            if (!request.IsValid()) return BadRequest();
            try
            {
                var uid = CurrentUserID;
                var doneByUser = await _context.Questions.Where(q => q.PollId == request.PollID)
                    .Join(_context.UserAnswers.Where(ua => ua.UserAnswerId == uid), q => q.QuestionId, ua => ua.QuestionId, (qr, uar) => uar.QuestionId)
                    .AnyAsync();

                if (doneByUser) return new GetPollResponse { Success = false, ErrorMessage = $"Already processed!" };


                
                var response = await _context.Polls.Where(p => p.PollId == request.PollID)
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
                response.Questions = await _context.Questions.Where(q => q.PollId == request.PollID).Select(q => new ApiQuestion
                {
                    QuestionId = q.QuestionId,
                    Title = q.Title,
                    Answers = _context.Answers.Where(a => a.QuestionId == q.QuestionId).Select(a => new ApiAnswer
                    {
                        Text = a.Text,
                        AnswerID = a.AnswerId
                    }).ToList()
                }).ToListAsync();
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, nameof(GetPoll));
                return new GetPollResponse { ErrorMessage = "Poll not found!" };
            }
        }
    }
}
