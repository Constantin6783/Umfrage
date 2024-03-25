
<script setup lang="ts">
    import { onMounted, ref, getCurrentInstance, computed } from "vue";
    import { Answer, Client, WritePollRequest, Question, GetPollRequest, EditPollResponse, BaseResponse } from '../service/Client'
    import { Modal } from 'bootstrap';
    import Swal from 'sweetalert2'

    const emit = defineEmits(['onClosed'])
    const props = defineProps({
        editPollId:{
            type:Number,
            required:false,
        }
    });
    const app = getCurrentInstance().parent.parent;
    const createPollRequest = ref(new WritePollRequest({
        questions: [new Question({ title: 'Neue Frage', answers: [new Answer({text:''}),new Answer({text:''})] })],
        title: 'Neue Umfrage',
        description: '',
        apiKey: 'ValidApiKey'
    }));
    const pollTitle = computed(() => (createPollRequest.value.pollId > 0 ? `Umfrage bearbeiten: ` : `Neue Umfrage: `) + createPollRequest.value.title);


    var myModal;
    async function closeModal(save) {
        if (save) {
            let error = ''
            let rawQuestions = createPollRequest.value.questions.map(a => a.title);
            let duplicatedQuestions = rawQuestions.filter((item, index) => rawQuestions.indexOf(item) !== index);

            if (createPollRequest.value.title === '') error = "Titel darf nicht leer sein!";            
            else if(rawQuestions.filter(q => q === '').length > 0) error='Es darf keine leeren Fragen geben!';
            else if(createPollRequest.value.questions.filter(q => q.answers.filter(a => a.text === '').length > 0).length > 0) error='Es darf keine leeren Antworten geben!';
            else if (duplicatedQuestions.length > 0) error = `Fragen müssen einzigartig sein!\nFolgende Fragen sind doppelt vorhanden: ${duplicatedQuestions.join(", ")}`
            else {

                let client = new Client()
                let resObj;
                app.props.isBusy = true;

                await client.createPoll(createPollRequest.value)
                    .then(r => resObj = r)
                    .catch(e => resObj = { success: false, errorMessage: e.message });

                app.props.isBusy = false;

                if (!resObj.success) error = resObj.errorMessage;
            }

            if (error !== '') {
                await Swal.fire('Fehler',error, 'error');
                return;
            }
        }
        myModal.hide();
        emit('onClosed', save)
    }
    onMounted(async () => {

        if (props.editPollId > 0) {
            let client = new Client();
            let resObj:EditPollResponse;
            let error: String;
            app.props.isBusy = true;

            await client.beginEditPoll(new GetPollRequest({ apiKey: 'ValidApiKey', pollId: props.editPollId }))
                .then(r => resObj = r)
                .catch(e => resObj = new BaseResponse({ success: false, errorMessage: e.message }));

            app.props.isBusy = false;

            if (!resObj.success) {
                await Swal.fire('Fehler', resObj.errorMessage, 'error');
                emit('onClosed');
                return;
            }
            else {
                createPollRequest.value.description = resObj.description;
                createPollRequest.value.title = resObj.title;
                createPollRequest.value.pollId = resObj.pollId;
                createPollRequest.value.questions = resObj.questions;
            }
        }


        myModal = new Modal(document.getElementById('dlg-create-poll'), { backdrop: false });
        myModal.show();
    })
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
                        <label for="poll-title" class="form-label">Title</label>
                        <input class="form-control" id="poll-title" v-model="createPollRequest.title" />
                    </div>
                    <div class="mb-3">
                        <label for="poll-description" class="form-label">Beschreibung</label>
                        <textarea class="form-control" id="poll-description" v-model="createPollRequest.description" placeholder="Optional" rows="5" />
                    </div>
                    <div>
                        <button class="btn btn-primary btn-sm mx-2" @click="createPollRequest.questions.push(new Question({title:'', answers:[new Answer(),new Answer()]}))">➕</button>
                        Fragen
                        <div class="card mt-3" v-for="question in createPollRequest.questions">
                            <div class="card-body">
                                <h5 class="card-title">
                                    <button v-if="createPollRequest.questions.length > 1" class="btn btn-danger btn-sm me-2 my-2" @click="createPollRequest.questions.splice(createPollRequest.questions.indexOf(question), 1)">🗑</button>
                                    Frage: <textarea class="form-control" v-model="question.title" placeholder="Frage" />
                                </h5>
                                <p>
                                    <button class="btn btn-primary btn-sm me-2 my-2" @click="question.answers.push(new Answer({text:'Neue Antwort ' + question.answers.length}))">➕</button>
                                    <label class="form-label mt-2">Antworten</label>
                                    <ul id="list-answers">
                                        <li v-for="answer in question.answers">
                                            <div class="d-flex mb-2">
                                                <div class="me-2 ">
                                                    <input class="form-control" v-model="answer.text" placeholder="Antwort" />
                                                </div>
                                                <button v-if="question.answers.length > 2" class="btn btn-danger" @click="question.answers.splice(question.answers.indexOf(answer), 1)">🗑</button>
                                            </div>
                                        </li>
                                    </ul>
                                </p>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" @click="closeModal(true)">Speichern</button>
                    <button type="button" class="btn btn-secondary" data-dismiss="modal" @click="closeModal(false)">Abbrechen</button>
                </div>
            </div>
        </div>
    </div>
</template>
<style scoped>
    ul {
        padding: unset;
    }
        ul li {
            list-style: none;
        }
        ul .form-control {
            width: 380px;
        }
</style>
