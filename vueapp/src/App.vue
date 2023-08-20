<script setup>
import HelloWorld from './components/HelloWorld.vue'
import ReportViewer from './components/ReportViewer.vue'
</script>

<template>
    <!--<HelloWorld />-->
    <ReportViewer/>
</template>

<style scoped>
    @import "https://cdn.boldreports.com/5.2.26/content/v2.0/tailwind-light/bold.report-viewer.min.css";
     html {
        height: 100%;
    }

    body {
        height: 100%;
        margin: 0;
        width: 100%;
        padding: 0;
    }
</style>
