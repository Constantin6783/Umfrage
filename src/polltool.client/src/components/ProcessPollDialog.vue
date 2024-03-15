<template>
    <div id="dlg-create-poll" class="modal fade" tabindex="-1" role="dialog">
        <div class="modal-dialog" role="document">
            <div class="modal-content">
                <div class="modal-header">
                    <h5 class="modal-title">{{pollTitle}} {{pollid}}</h5>
                </div>
                <div class="modal-body">
                    <div v-if="pollDescription" class="mb-3">
                        <label for="poll-description" class="form-label">{{pollDescription}}</label>
                        <textarea class="form-control" id="poll-description" v-model="description" placeholder="Optional" rows="5" />
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

<script setup lang="ts">
    import { onMounted, ref } from "vue";
    import { Answer, Client, CreatePollRequest, GetPollRequest, GetPollResponse } from '../service/Client'
    import { Modal } from 'bootstrap';
import Swal from "sweetalert2";

    const emit = defineEmits(['onClosed']);
    const props = defineProps({
        pollid: {
            type: Number
        }
    })

    const pollTitle = ref("");
    const pollDescription = ref("");
    var myModal;
    onMounted(async () => {
        let resObj: GetPollResponse;
        let client = new Client();
        window.isBusy(true);
        await client.getPoll(new GetPollRequest({ apiKey: 'ValidApiKey', pollID: props.pollid }))
            .then(r => resObj = r)
            .catch(e => resObj = new GetPollResponse({ success: false, errorMessage: e.message }));
        window.isBusy(false);

        if (resObj.success) {
            myModal = new Modal(document.getElementById('dlg-create-poll'), { backdrop: false });
            myModal.show();
        } else {
            Swal.fire(resObj.errorMessage);
            emit('onClosed', false)
        }
    })

    function closeModal(save) {
        myModal.hide();
        emit('onClosed', save)
    }
</script>

<style>
</style>