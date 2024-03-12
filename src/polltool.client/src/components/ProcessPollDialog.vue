<template>
    <div id="dlg-create-poll" class="modal fade" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">{{polltitle}}</h5>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label for="poll-title" class="form-label">Title</label>
                        <input class="form-control" id="poll-title" v-model="title" />
                    </div>
                    <div class="mb-3">
                        <label for="poll-description" class="form-label">Beschreibung</label>
                        <textarea class="form-control" id="poll-description" v-model="description" placeholder="Optional" rows="5" />
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

<script lang="ts">
    import { defineComponent } from 'vue';
    
</script>

<style>
</style>