import './assets/main.css'
import { createApp } from 'vue'
import App from './App.vue'
import CreatePollDialog from './components/CreatePollDialog.vue'
import PollOverview from './components/PollOverview.vue'
import ProcessPollDialog from './components/ProcessPollDialog.vue'
import StatisticsPollDialog from './components/StatisticsPollDialog.vue'


const app = createApp(App);

app.component('ProcessPollDialog', ProcessPollDialog);
app.component('StatisticsPollDialog', StatisticsPollDialog);
app.component('CreatePollDialog', CreatePollDialog)
app.component('PollOverview', PollOverview)
app.mount('#app');
