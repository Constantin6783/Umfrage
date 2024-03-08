<script setup>
    import { onMounted, resolveComponent, ref, createBlock } from "vue";
    import CreatePollDialog from './CreatePollDialog.vue'

    const emit = defineEmits(['isBusy'])

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
</script>

<template>
    <div class="row">
        <button class="btn btn-primary col-lg-3" @click="addPoll">Neue Umfrage</button>
    </div>
    <div class="row">
        <table class="table">
            <thead>
                <tr>
                    <th>Titel</th>
                    <th colspan="3">Beschreibung</th>
                </tr>
            </thead>
            <tbody>
                <tr v-for="poll in polls">
                    <td>{{poll.title}}</td>
                    <td>{{poll.description}}</td>
                    <td v-if="!poll.doneByUser" width="100px"><button class="btn btn-primary" @click="processPoll">Teilnehmen</button></td>
                    <td v-if="poll.doneByUser" width="100px"><button class="btn btn-primary disabled">Teilgenommen</button></td>
                    <td width="100px"><button class="btn btn-primary">Statistiken</button></td>
                </tr>
            </tbody>
        </table>

    </div>
    <CreatePollDialog v-if="createPoll" @onClosed="addPoll" />
</template>


<style scoped>
</style>
