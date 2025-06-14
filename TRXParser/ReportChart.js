// Data will be injected by C# before this script is rendered
// const passedCount = /*INJECTED_PASSED_COUNT*/;
// const failedCount = /*INJECTED_FAILED_COUNT*/;
// const timeoutCount = /*INJECTED_TIMEOUT_COUNT*/;
// const reportDate = "/*INJECTED_REPORT_DATE*/";

google.load('visualization', '1.0', { 'packages': ['corechart'] });
google.setOnLoadCallback(drawChart);

function drawChart() {
    var data = new google.visualization.DataTable();
    data.addColumn('string', 'Results');
    data.addColumn('number', 'Count');
    data.addRows([
        ['Passed', passedCount],
        ['Failed', failedCount],
        ['Timeout', timeoutCount]
        // Potentially add other statuses if your TestOutcome enum and parsing supports them
    ]);

    var options = {
        'title': 'Test Report - ' + reportDate,
        'width': 450, // Adjusted width slightly
        'height': 300,
        'is3D': true,
        'colors': ['#4CAF50', '#F44336', '#FFC107', '#2196F3'] // Green, Red, Amber, Blue for other potential statuses
    };

    var chart = new google.visualization.PieChart(document.getElementById('chart_div'));
    chart.draw(data, options);
}
