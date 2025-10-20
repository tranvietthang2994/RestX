$(document).ready(function () {
    console.log('Dashboard.js loaded');

    // Don't create profit chart here - let the page handle it
    // Just initialize other charts

    // =====================================
    // Breakup (Yearly Revenue)
    // =====================================
    function initializeYearlyBreakup() {
        if (typeof yearlyRevenue !== 'undefined' && yearlyRevenue) {
            const years = Object.keys(yearlyRevenue).sort();
            const revenues = years.map(year => yearlyRevenue[year]);

            console.log('Yearly data:', { years, revenues });

            var breakup = {
                color: "#adb5bd",
                series: revenues,
                labels: years,
                chart: {
                    width: 180,
                    type: "donut",
                    fontFamily: "Plus Jakarta Sans', sans-serif",
                    foreColor: "#adb0bb",
                },
                plotOptions: {
                    pie: {
                        startAngle: 0,
                        endAngle: 360,
                        donut: {
                            size: '75%',
                        },
                    },
                },
                stroke: {
                    show: false,
                },

                dataLabels: {
                    enabled: false,
                },

                legend: {
                    show: false,
                },
                colors: ["#5D87FF", "#ecf2ff", "#F9F9FD"],

                responsive: [
                    {
                        breakpoint: 991,
                        options: {
                            chart: {
                                width: 150,
                            },
                        },
                    },
                ],
                tooltip: {
                    theme: "dark",
                    fillSeriesColor: false,
                    formatter: function (value, { seriesIndex, w }) {
                        const year = w.config.labels[seriesIndex];
                        return `${year}: $${value.toLocaleString()}`;
                    }
                },
            };

            var chart = new ApexCharts(document.querySelector("#breakup"), breakup);
            chart.render();
        } else {
            console.log('No yearly revenue data available');
        }
    }

    // =====================================
    // Earning (Monthly Earnings Trend)
    // =====================================
    function initializeMonthlyEarnings() {
        if (typeof monthlyEarningsTrend !== 'undefined' && monthlyEarningsTrend && monthlyEarningsTrend.length > 0) {
            // Use the dedicated monthly earnings trend data
            console.log('Using monthly earnings trend data:', monthlyEarningsTrend);

            // Generate month labels for the last 7 months
            const monthLabels = [];
            const now = new Date();
            for (let i = 6; i >= 0; i--) {
                const date = new Date(now.getFullYear(), now.getMonth() - i, 1);
                monthLabels.push(date.toLocaleDateString('en-US', { month: 'short' }));
            }

            var earning = {
                chart: {
                    id: "sparkline3",
                    type: "area",
                    height: 60,
                    sparkline: {
                        enabled: true,
                    },
                    group: "sparklines",
                    fontFamily: "Plus Jakarta Sans', sans-serif",
                    foreColor: "#adb0bb",
                },
                series: [
                    {
                        name: "Earnings",
                        color: "#49BEFF",
                        data: monthlyEarningsTrend,
                    },
                ],
                stroke: {
                    curve: "smooth",
                    width: 2,
                },
                fill: {
                    colors: ["#f3feff"],
                    type: "solid",
                    opacity: 0.05,
                },

                markers: {
                    size: 0,
                },
                tooltip: {
                    theme: "dark",
                    fixed: {
                        enabled: true,
                        position: "right",
                    },
                    x: {
                        show: false,
                    },
                    formatter: function (value, { seriesIndex, dataPointIndex, w }) {
                        const month = monthLabels[dataPointIndex] || 'Month';
                        return `${month}: $${value.toLocaleString()}`;
                    }
                },
            };

            new ApexCharts(document.querySelector("#earning"), earning).render();

        } else if (typeof monthlyChartData !== 'undefined' && monthlyChartData) {
            // Fallback: Extract monthly totals from monthlyChartData
            const monthlyTotals = [];
            const monthLabels = [];

            // Get the last 7 months or available months
            const sortedMonths = Object.keys(monthlyChartData).sort().slice(-7);

            for (const monthKey of sortedMonths) {
                const monthData = monthlyChartData[monthKey];
                const total = monthData.reduce((sum, day) => sum + day.Profit, 0);
                monthlyTotals.push(total);

                // Format month label
                const [year, month] = monthKey.split('-');
                const date = new Date(year, month - 1, 1);
                monthLabels.push(date.toLocaleDateString('en-US', { month: 'short' }));
            }

            console.log('Using fallback monthly earnings data:', { monthLabels, monthlyTotals });

            var earning = {
                chart: {
                    id: "sparkline3",
                    type: "area",
                    height: 60,
                    sparkline: {
                        enabled: true,
                    },
                    group: "sparklines",
                    fontFamily: "Plus Jakarta Sans', sans-serif",
                    foreColor: "#adb0bb",
                },
                series: [
                    {
                        name: "Earnings",
                        color: "#49BEFF",
                        data: monthlyTotals,
                    },
                ],
                stroke: {
                    curve: "smooth",
                    width: 2,
                },
                fill: {
                    colors: ["#f3feff"],
                    type: "solid",
                    opacity: 0.05,
                },

                markers: {
                    size: 0,
                },
                tooltip: {
                    theme: "dark",
                    fixed: {
                        enabled: true,
                        position: "right",
                    },
                    x: {
                        show: false,
                    },
                    formatter: function (value, { seriesIndex, dataPointIndex, w }) {
                        const month = monthLabels[dataPointIndex] || 'Month';
                        return `${month}: $${value.toLocaleString()}`;
                    }
                },
            };

            new ApexCharts(document.querySelector("#earning"), earning).render();

        } else if (typeof currentMonthRevenue !== 'undefined' && typeof previousMonthRevenue !== 'undefined') {
            // Final fallback: use simple current vs previous month data
            console.log('Using simple current vs previous month data');

            var earning = {
                chart: {
                    id: "sparkline3",
                    type: "area",
                    height: 60,
                    sparkline: {
                        enabled: true,
                    },
                    group: "sparklines",
                    fontFamily: "Plus Jakarta Sans', sans-serif",
                    foreColor: "#adb0bb",
                },
                series: [
                    {
                        name: "Earnings",
                        color: "#49BEFF",
                        data: [previousMonthRevenue, currentMonthRevenue],
                    },
                ],
                stroke: {
                    curve: "smooth",
                    width: 2,
                },
                fill: {
                    colors: ["#f3feff"],
                    type: "solid",
                    opacity: 0.05,
                },

                markers: {
                    size: 0,
                },
                tooltip: {
                    theme: "dark",
                    fixed: {
                        enabled: true,
                        position: "right",
                    },
                    x: {
                        show: false,
                    },
                    formatter: function (value, { seriesIndex, dataPointIndex, w }) {
                        const months = ['Last Month', 'This Month'];
                        const month = months[dataPointIndex] || 'Month';
                        return `${month}: $${value.toLocaleString()}`;
                    }
                },
            };

            new ApexCharts(document.querySelector("#earning"), earning).render();

        } else {
            console.log('No monthly earnings data available');
        }
    }

    // Initialize charts after a short delay to ensure data is available
    setTimeout(() => {
        initializeYearlyBreakup();
        initializeMonthlyEarnings();
    }, 500);
});