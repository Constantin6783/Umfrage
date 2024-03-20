<script setup lang="ts">
    import { computed, getCurrentInstance, onMounted, reactive, ref, resolveComponent } from "vue";
    import { Answer, Question, AnsweredQuestion, Client, CreatePollRequest, GetPollRequest, GetPollResponse, ProcessPollRequest, BaseResponse } from '../service/Client'
    import { Modal } from 'bootstrap';
    import Swal from "sweetalert2";


    const app = getCurrentInstance().parent.parent;
    const emit = defineEmits(['onClosed']);
    const props = defineProps({
        pollid: {
            type: Number
        }
    })

    const poll = reactive<GetPollResponse>(new GetPollResponse())
    const pollRequest = reactive<ProcessPollRequest>(new ProcessPollRequest({ apiKey: "ValidApiKey", pollId: props.pollid, answereds: [] }));
    const pollTitle = computed(() => poll.poll?.title ?? '');
    const pollDescription = computed(() => poll.poll?.description ?? '');
    const allAnswered = computed(() => pollRequest.answereds.filter((p) => p.selectedAnswer == 0).length == 0)
    var myModal;
    function isBusys(s) {

    }
    onMounted(async () => {
        let resObj: GetPollResponse;
        let client = new Client();
        app.props.isBusy = true;
        await client.getPoll(new GetPollRequest({ apiKey: 'ValidApiKey', pollId: props.pollid }))
            .then(r => resObj = r)
            .catch(e => resObj = new GetPollResponse({ success: false, errorMessage: e.message }));
        app.props.isBusy = false;

        if (resObj.success) {
            poll.poll = resObj.poll;
            poll.questions = resObj.questions;
            pollRequest.answereds = resObj.questions.map(q => new AnsweredQuestion({ questionId: q.questionId, selectedAnswer: 0 }));

            myModal = new Modal(document.getElementById('dlg-create-poll'), { backdrop: false });
            myModal.show();
        } else {
            Swal.fire(resObj.errorMessage);
            emit('onClosed', false)
        }
    })

    async function closeModal(save) {
        if (save) {
            let client = new Client();
            let resObj: BaseResponse;

            app.props.isBusy = true;

            await client.processPoll(pollRequest)
                .then(r => resObj = r)
                .catch(e => resObj = new BaseResponse({ success: false, errorMessage: e.message }));

            app.props.isBusy = false;
            if (!resObj.success) {
                Swal.fire(resObj.errorMessage);
                return;
            }
        }
        myModal.hide();
        emit('onClosed', save)
    }
</script>

<template>
    <div id="dlg-create-poll" class="modal fade" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">{{pollTitle}}</h5>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label v-if="pollDescription" for="poll-description" class="form-label">{{pollDescription}}</label>

                        <div v-for="(question, idxQ) in poll.questions">
                            <label>{{question.title}}</label>
                            <div v-for="answer in question.answers" class="form-check">
                                <input class="form-check-input" v-model="pollRequest.answereds[idxQ].selectedAnswer" :value="answer.answerId" type="radio" :name="'answerfor-' + question.questionId" :id="'answer-' + answer.answerId">
                                <label class="form-check-label" :for="'answer-' + answer.answerId">
                                    {{answer.text}}
                                </label>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" :disabled="!allAnswered" class="btn btn-primary" @click="closeModal(true)">Absenden</button>
                    <button type="button" class="btn btn-secondary" data-dismiss="modal" @click="closeModal(false)">Abbrechen</button>
                </div>
            </div>
        </div>
    </div>
</template>

<style>
</style>