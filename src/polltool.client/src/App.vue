<script setup>
    import { onMounted, resolveComponent, ref, defineComponent } from "vue";
    import PollOverview from './components/PollOverview.vue'
    const isBusy = ref(true);

    window.PostData = async function PostData(url, data) {
        let resObj;
        isBusy.value = true;
        await fetch(url, {
            method: 'POST',
            body: JSON.stringify(data),
            headers: {
                'Content-Type': 'application/json',
            }
        })
            .then(async response => resObj = await response.json())
            .catch(error => resObj = { success: false, errorMessage: error });

        isBusy.value = false;
        return resObj;
    }

</script>

<template>
    <!-- Navigation Bar -->
    <nav class="navbar navbar-expand-lg navbar-light bg-light">
        <div class="container-fluid">
            <a class="navbar-brand" href="#">Umfragetool</a>
            <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarSupportedContent" aria-controls="navbarSupportedContent" aria-expanded="false" aria-label="Toggle navigation">
                <span class="navbar-toggler-icon"></span>
            </button>
            <div class="collapse navbar-collapse" id="navbarSupportedContent">
                <ul class="navbar-nav me-auto mb-2 mb-lg-0">
                    <li class="nav-item">
                        <a class="nav-link active" aria-current="page" href="#" @click="mountOverview">Umfragen</a>
                    </li>
                </ul>
            </div>
        </div>
    </nav>
    <div id="content" class="mx-5">
        <PollOverview ref="pollOverview" />
        <div id="busy-shadow" v-if="isBusy">
            <div class="spinner-border" role="status">
                <span class="visually-hidden">Loading...</span>
            </div>
        </div>
    </div>
</template>

<style scoped>
    #busy-shadow {
        background: #80808099;
        position: absolute;
        top: 0;
        left: 0;
        right: 0;
        bottom: 0;
        z-index: 9999;
    }

        #busy-shadow .spinner-border {
            position: absolute;
            top: 50%;
            left: 50%;
        }
</style>
