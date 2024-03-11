<script setup>
    import { onMounted, resolveComponent, ref, createBlock } from "vue";
    import CreatePollDialog from './CreatePollDialog.vue'
    import Swal from 'sweetalert2'    

    const createPoll = ref(false);
    const polls = ref([])

    onMounted(() => {
        updatePolls();
    })

    async function updatePolls() {
        let resObj = await window.PostData('http://192.168.178.23:5134/api/Polls/GetPolls', { apiKey: 'test' })
        if (resObj.success) polls.value = resObj.polls;
        else alert(resObj.errorMessage);
    }

    async function addPoll(refresh) {
        createPoll.value = !createPoll.value;
        if (typeof refresh === 'boolean' && refresh)
            updatePolls();

    }
    async function deletePoll(poll) {
        var deletePoll = (await Swal.fire({
            title: 'Nachfrage',
            text: `Möchten Sie die Umfrage '${poll.title}' wirklich löschen?`,
            confirmButtonText: 'Löschen',
            cancelButtonText: 'Abbrechen',
            showCancelButton: true
        })).isConfirmed;

        if (deletePoll) {
            var resObj = await window.PostData('http://192.168.178.23:5134/api/Polls/DeletePoll', { PollID: poll.pollId, apiKey: 'test' });
            if (resObj.success) Swal.fire({
                                  title: "Erfolg!",
                                  text: "Umfrage erfolgreich gelöscht!",
                                  icon: "success"
                                });
            else alert(resObj.errorMessage);
            updatePolls();
        }
    }
    
</script>

<template>
    <div class="row">
        <button class="btn btn-primary col-lg-1" @click="addPoll">Neue Umfrage</button>
    </div>
    <div class="row">
        <table class="table">
            <thead>
                <tr>
                    <th>Titel</th>
                    <th colspan="4">Beschreibung</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="poll in polls">
                    <td>{{poll.title}}</td>
                    <td>{{poll.description}}</td>
                    <td v-if="!poll.doneByUser" width="150px"><button class="btn btn-primary btn-process" @click="processPoll">Teilnehmen</button></td>
                    <td v-if="poll.doneByUser" width="150px"><button class="btn btn-primary btn-processed disabled">Teilgenommen</button></td>
                    <td width="100px"><button class="btn btn-primary">Statistiken</button></td>
                    <td width="100px"><button v-if="poll.ownedByUser" class="btn btn-danger warning" @click="deletePoll(poll)">🗑</button></td>
                </tr>
            </tbody>
        </table>

    </div>
    <CreatePollDialog v-if="createPoll" @onClosed="addPoll" />
</template>


<style scoped>
    table .btn.btn-process,
    table .btn.btn-processed {
        width: 150px;
    }
</style>
