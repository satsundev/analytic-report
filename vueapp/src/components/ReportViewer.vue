<template>
    <div id="viewer" style="height: 1000px; width: 100%"></div>
</template>

<script setup>
    import { onMounted } from 'vue'
    import $ from 'jquery';
    import '@boldreports/javascript-reporting-controls/Scripts/v2.0/common/bold.reports.common.min';
    import '@boldreports/javascript-reporting-controls/Scripts/v2.0/common/bold.reports.widgets.min';
    import '@boldreports/javascript-reporting-controls/Scripts/v2.0/bold.report-viewer.min';
    //Data-Visualization
    import '@boldreports/javascript-reporting-controls/Scripts/data-visualization/ej.bulletgraph.min';
    import '@boldreports/javascript-reporting-controls/Scripts/data-visualization/ej.chart.min';

    onMounted(() => {
        $("#viewer").boldReportViewer({
            reportServiceUrl: "/api/ReportViewer"
        });
    })
</script>