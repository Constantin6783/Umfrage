<script setup lang="ts">
    import { onMounted, resolveComponent, ref, createBlock, getCurrentInstance } from "vue";
    import CreatePollDialog from './CreatePollDialog.vue'
    import ProcessPollDialog from './ProcessPollDialog.vue'
    import StatisticsPollDialog from './StatisticsPollDialog.vue'
    import Swal from 'sweetalert2'
    import { Client, BaseRequest, DeletePollRequest } from '../service/Client'
    const parent = getCurrentInstance().parent;
    const createPoll = ref(false);
    const processPollId = ref(-1);
    const statisticsPollId = ref(-1);
    const polls = ref([])


    onMounted(() => {
        updatePolls();
    })

    async function updatePolls() {

        let client = new Client()
        let resObj;

        parent.props.isBusy = true;

        await client.getPolls(new BaseRequest({ apiKey: 'ValidApiKey' }))
            .then(r => resObj = r)
            .catch(e => resObj = { success: false, errorMessage: e.message });

        parent.props.isBusy = false;

        if (resObj.success) polls.value = resObj.polls;
        else alert(resObj.errorMessage);
    }

    async function addPoll(refresh) {
        createPoll.value = !createPoll.value;
        if (typeof refresh === 'boolean' && refresh)
            updatePolls();
    }

    async function onProcessPoll(param) {

        if (typeof param === 'number') processPollId.value = param;
        else processPollId.value = -1;

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
            parent.props.isBusy = true;

            await client.deletePoll(new DeletePollRequest({ pollId: poll.pollId, apiKey: 'ValidApiKey' }))
                .then(r => resObj = r)
                .catch(e => resObj = { success: false, errorMessage: e.message });

            parent.props.isBusy = false;

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
    <div class="row my-3">
        <button class="btn btn-primary col-lg-1" @click="addPoll">Neue Umfrage</button>
        <button class="btn btn-primary col-lg-1 mx-3" @click="updatePolls">Aktualisieren</button>
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
                    <td v-else="" width="150px"><button class="btn btn-primary btn-processed disabled">Teilgenommen</button></td>
                    <td width="100px"><button class="btn btn-primary" @click="statisticsPollId = poll.pollId">Statistiken</button></td>
                    <td width="100px"><button v-if="poll.ownedByUser" class="btn btn-danger warning" @click="deletePoll(poll)">🗑</button></td>
                </tr>
            </tbody>
        </table>

    </div>
    <CreatePollDialog v-if="createPoll" @onClosed="addPoll" />
    <ProcessPollDialog v-if="processPollId > 0" @onClosed="onProcessPoll" :pollid="processPollId" />
    <StatisticsPollDialog v-if="statisticsPollId > 0" @onClosed="statisticsPollId = -1" :pollid="statisticsPollId" />
</template>


<style scoped>
    table .btn.btn-process,
    table .btn.btn-processed {
        width: 150px;
    }
</style>
