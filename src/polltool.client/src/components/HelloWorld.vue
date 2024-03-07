<script setup>
    import { onMounted, resolveComponent, ref, createBlock } from "vue";
    import * as bootstrap from 'bootstrap'
    import CreatePollDialog from './CreatePollDialog.vue'

    const createPoll = ref(false);
    const polls = ref([])

    onMounted(() => {
        updatePolls();
    })

    async function updatePolls() {
        let resObj;
        
        await fetch('http://localhost:5134/api/Polls', {
            method: 'POST',
            body: JSON.stringify({ apiKey: 'Test' }),
            headers: {
                'Content-Type': 'application/json',
            }
        })
            .then(async response => resObj = await response.json())
            .catch(error => resObj = {success:false,error});
        if (resObj.success) {
            polls.value = resObj.polls;
        } else alert(resObj.error);
    }    

    async function addPoll() {
        alert();
        createPoll.value = !createPoll.value;
    }
</script>

<template>
    <div class="row">
        <button class="btn btn-primary col-lg-3" @click="addPoll">Neue Umfrage</button>
    </div> 
    <div class="row">
        <ul>
            <li v-for="poll in polls">
                <a href="#" :title="poll.description" :data-poll="poll">{{poll.title}}</a>
                <button class="btn btn-primary">Teilnehmen</button>
            </li>
        </ul>
    </div>
    <CreatePollDialog v-if="createPoll" @closeDialog="addPoll"/>
</template>


<style scoped>
h1 {
  font-weight: 500;
  font-size: 2.6rem;
  position: relative;
  top: -10px;
}

h3 {
  font-size: 1.2rem;
}

.greetings h1,
.greetings h3 {
  text-align: center;
}

@media (min-width: 1024px) {
  .greetings h1,
  .greetings h3 {
    text-align: left;
  }
}
ul{
    list-style:none;
}
</style>
