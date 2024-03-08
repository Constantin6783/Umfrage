
<script setup>
    import { onMounted, ref } from "vue";
    import { Modal } from 'bootstrap';

    const emit = defineEmits(['onClosed'])
    const title = ref('');
    const description = ref('');
    const answers = ref([]);

    var myModal;
    async function closeModal(save) {
        if(save){            
            let error = ''
            let rawAnswers = answers.value.map(a => a.text);
            let duplicatedAnswers = rawAnswers.filter((item, index) => rawAnswers.indexOf(item) !== index);

            if(title.value === '')error = "Titel darf nicht leer sein!";
            else if(answers.value.length < 2) error = "Es müssen mind. 2 Antworten verfügbar sein!";
            else if (duplicatedAnswers.length > 0) error = `Antworten müssen einzigartig sein!\nFolgende Anworten sind doppelt vorhanden: ${duplicatedAnswers.join(", ")}`
            else
            {
                
                let resObj = await window.PostData('http://192.168.178.23:5134/api/Polls/CreatePoll',
                    {
                        title: title.value,
                        description: description.value,
                        answers: answers.value,
                        apiKey: 'test'
                    });

                if (!resObj.success) error = resObj.errorMessage;
            }

            if(error !== ''){
                alert(error);
                return;
            }
        } 
        myModal.hide();
        emit('onClosed', save)
    }
    onMounted(() => {
        myModal = new Modal(document.getElementById('dlg-create-poll'), { backdrop: false });
        myModal.show();
    })
</script>

<template>
    <div id="dlg-create-poll" class="modal fade" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document"> 
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Neue Umfrage</h5>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label for="poll-title" class="form-label">Title</label>
                        <input class="form-control" id="poll-title" v-model="title" />
                    </div>
                    <div class="mb-3">
                        <label for="poll-description" class="form-label">Beschreibung</label>
                        <textarea class="form-control" id="poll-description" v-model="description" placeholder="Optional" rows="5"/>
                    </div>

                    <div class="d-flex mb-2">
                        <label class="form-label mt-2">Antworten</label>
                        <button class="btn btn-primary ms-2" @click="answers.push({text:'Neue Antwort ' + answers.length})">Neue Antwort</button>
                    </div>
                    <ul id="list-answers">
                        <li v-for="question in answers">
                            <div class="d-flex mb-2">
                                <div class="me-2 ">
                                    <input class="form-control" v-model="question.text" />
                                </div>
                                <button class="btn btn-danger" @click="answers.splice(answers.indexOf(question), 1)">🗑</button>
                            </div>
                        </li>
                    </ul>
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
    ul{
        padding:unset;
    }
    ul li {
        list-style: none;
    }
    ul .form-control{
        width:410px;
    }
</style>