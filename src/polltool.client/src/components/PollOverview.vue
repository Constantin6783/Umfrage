<script setup lang="ts">
    import { onMounted, resolveComponent, ref, createBlock } from "vue";
    import CreatePollDialog from './CreatePollDialog.vue'
    import  ProcessPollDialog  from './ProcessPollDialog.vue'
    import Swal from 'sweetalert2'
    import { Client, BaseRequest, DeletePollRequest } from '../service/Client'

    const createPoll = ref(false);
    const processPollID = ref(-1);
    const polls = ref([])
    

    onMounted(() => {
        updatePolls();
    })

    async function updatePolls() {
        
        let client = new Client()
        let resObj;
        window.isBusy(true);

        await client.getPolls(new BaseRequest({ apiKey:'ValidApiKey' }))
            .then(r => resObj = r)
            .catch(e => resObj = {success: false, errorMessage: e.message });

        window.isBusy(false);

        if (resObj.success) polls.value = resObj.polls;
        else alert(resObj.errorMessage);
    }

    async function addPoll(refresh) {
        createPoll.value = !createPoll.value;
        if (typeof refresh === 'boolean' && refresh)
            updatePolls();
    }

    async function onProcessPoll(param) {
        
        if (typeof param === 'number') processPollID.value = param;
        else processPollID.value = -1;

        if (typeof param === 'boolean' && param)
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

            let client = new Client()
            let resObj;
            window.isBusy(true);

            await client.deletePoll(new DeletePollRequest({ pollID: poll.pollId, apiKey: 'ValidApiKey' }))
                .then(r => resObj = r)
                .catch(e => resObj = { success: false, errorMessage: e.message });

            window.isBusy(false);

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
                    <td v-if="!poll.doneByUser" width="150px"><button class="btn btn-primary btn-process" @click="onProcessPoll(poll.pollId)">Teilnehmen</button></td>
                    <td v-if="poll.doneByUser" width="150px"><button class="btn btn-primary btn-processed disabled">Teilgenommen</button></td>
                    <td width="100px"><button class="btn btn-primary">Statistiken</button></td>
                    <td width="100px"><button v-if="poll.ownedByUser" class="btn btn-danger warning" @click="deletePoll(poll)">🗑</button></td>
                </tr>
            </tbody>
        </table>

    </div>
    <CreatePollDialog v-if="createPoll" @onClosed="addPoll" />
    <ProcessPollDialog v-if="processPollID > 0" @onClosed="onProcessPoll" :pollid="processPollID"/>
</template>


<style scoped>
    table .btn.btn-process,
    table .btn.btn-processed {
        width: 150px;
    }
</style>
