<script setup lang="ts">
    import { computed, getCurrentInstance, onMounted, reactive, ref, resolveComponent } from "vue";
    import { BaseResponse, Client, GetPollRequest, PollStatisticResponse } from '../service/Client'
    import { Modal } from 'bootstrap';
    import Swal from 'sweetalert2';


    const app = getCurrentInstance().parent.parent;
    const emit = defineEmits(['onClosed']);
    const props = defineProps({
        pollid: {
            type: Number
        }
    })

    const pollStatistics = ref(new PollStatisticResponse());
    var myModal;

    onMounted(async () => {
        let resObj: PollStatisticResponse;
        let client = new Client(); 
        app.props.isBusy = true;
        await client.pollStatistic(new GetPollRequest({ apiKey: 'ValidApiKey', pollId: props.pollid }))
            .then(r => resObj = r)
            .catch(e => resObj = new PollStatisticResponse({ success: false, errorMessage: e.message }));
        app.props.isBusy = false;

        if (resObj.success) {
            myModal = new Modal(document.getElementById('dlg-statistics-poll'), { backdrop: false });
            myModal.show();
            pollStatistics.value = resObj;
        } else {
            Swal.fire(resObj.errorMessage);
            emit('onClosed')
        }
    })

    async function closeModal() {
        myModal.hide();
        emit('onClosed')
    }
</script>

<template>
    <div id="dlg-statistics-poll" class="modal fade modal-xl" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">Statistiken zur Umfrage '{{pollStatistics.pollTitle}}'</h5>
                </div>
                <div class="modal-body">
                    <label>{{pollStatistics.pollDescription}}</label>
                    <div :class="pollStatistics.questions.indexOf(q)>0? 'mt-5' :''" v-for="q in pollStatistics.questions">
                        <h5>{{q.question}}</h5>
                        <table class="table table-bordered">
                            <thead>
                                <tr>
                                    <th width="100">Antwort</th>
                                    <th v-for="a in q.answers">
                                        {{a.answer}}
                                    </th>
                                </tr>
                            </thead>
                            <tbody>
                                <tr>
                                    <td width="100">Anzahl</td>
                                    <td v-for="a in q.answers">
                                        {{a.count}}
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-secondary" data-dismiss="modal" @click="closeModal()">Schließen</button>
                </div>
            </div>
        </div>
    </div>
</template>

<style>
</style>