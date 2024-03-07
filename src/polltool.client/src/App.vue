<script setup>
    import { onMounted, resolveComponent, ref } from "vue";
    
    import HelloWorld from './components/HelloWorld.vue'
    import TheWelcome from './components/TheWelcome.vue'
    import { defineAsyncComponent } from 'vue'
    const showpolls = ref(true);
    function mountOverview() {
        showpolls.value = !showpolls.value;
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
                    <li class="nav-item">
                        <a class="nav-link" href="#">Link</a>
                    </li>
                </ul>
            </div>
        </div>
    </nav>
    <div class="row">
        <div id="content" class="col col-lg-6 offset-lg-3" style="background:pink">
            <HelloWorld v-if="showpolls"/>
        </div>
    </div>
</template>

<style scoped>
</style>
