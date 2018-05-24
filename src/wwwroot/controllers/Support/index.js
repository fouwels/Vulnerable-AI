'use strict';
$.getJSON("/api/v0/data", function (data) {
    index(data)
});

var sentimentData = [
	0.1, 0.05, 0.1, 0.15, 0.1, 0.05, 0.1
];

var frequencyData = [
	1, 0, 0, 1, 0, 2, 1, 2, 3, 4, 6, 7
];

var ctxf = document.getElementById("supportFrequency").getContext('2d');
var frequencyChart = new Chart(ctxf, {
	type: 'line',

	data: {
		labels: ['Jun', 'Jul', 'Aug', 'Sep','Oct', 'Nov', 'Dec', 'Jan', 'Feb', 'Mar', 'Apr', 'Now', ''],
		datasets: [{
			label: 'Support Frequency',
			data: [0],
			backgroundColor: [
				'rgba(255, 99, 132, 0.2)'
			],
			borderColor: [
				'rgba(255,99,132,1)',
			],
			borderWidth: 1
		}]
	},
	options: {
		scales: {
			yAxes: [{
				ticks: {
					beginAtZero: false,
				}
			}]
		},
		elements: {
			line: {
				lineTension: 0
			}
		}
	}
});

var ctxs = document.getElementById("sentimentChart").getContext('2d');
var chart = new Chart(ctxs, {
	type: 'line',
	
	data: {
		labels: ['1', '2', '3', '4', '5', '6', '7', 'Now', ''],
		datasets: [{
			label: 'Support Sentiment',
			data: [0],
			backgroundColor: [
				'rgba(255, 99, 132, 0.2)'
			],
			borderColor: [
				'rgba(255,99,132,1)',
			],
			borderWidth: 1
		}]
	},
	options: {
		scales: {
			yAxes: [{
				ticks: {
					beginAtZero: false,
					min: -1,
					max: 1

				}
			}]
		},
		elements: {
			line: {
				//lineTension: 0
			}
		}
	}
});


var ctxd = document.getElementById("energyDelta").getContext('2d');
var energyDeltaChart = new Chart(ctxd, {
	type: 'line',

	data: {
		labels: ['1'],
		datasets: [{
			label: 'Customer Energy Delta',
			data: [0],
			backgroundColor: [
				'rgba(0, 99, 132, 0)',
				'rgba(255, 206, 86, 0.2)',
			],
			borderColor: [
				'rgba(54, 162, 235, 1)',
			],
			borderWidth: 1
		}]
	},
	options: {
		scales: {
			yAxes: [{
				ticks: {
					beginAtZero: false,
					min: -0.5,
					max: 0.5
				}
			}]
		},
		elements: {
			line: {
				//lineTension: 0
			},
			point: {
				//radius: 0
			}
		}
	}
});

var ctxe = document.getElementById("energyChart").getContext('2d');
var energyChart = new Chart(ctxe, {
	type: 'line',

	data: {
		labels: ['1'],
		datasets: [{
			label: 'Customer Current 7 Day Energy Usage (Gilmond)',
			data: [0],
				backgroundColor: [
					'rgba(0, 99, 132, 0)',
				'rgba(255, 206, 86, 0.2)',
			],
			borderColor: [
				'rgba(54, 162, 235, 1)',
			],
			borderWidth: 1
			}]
	},
	options: {
		scales: {
			yAxes: [{
				ticks: {
					beginAtZero: false,
					min: 0,
					max: 1
				}
			}]
		},
		elements: {
			line: {
				//lineTension: 0
			},
			point: {
				radius: 0
			}
		}
	}
});

var ctxea = document.getElementById("energyAverageChart").getContext('2d');
var energyAverageChart = new Chart(ctxea, {
	type: 'line',

	data: {
		labels: ['1'],
		datasets: [{
			label: 'Customer Typical 7 Day Energy Usage (Gilmond)',
			data: [0],
			backgroundColor: [
				'rgba(0, 0, 132, 0)',
				'rgba(255, 99, 132, 0.2)'
			],
			borderColor: [
				'rgba(255,99,132,1)',
			],
			borderWidth: 1
		}]
	},
	options: {
		scales: {
			yAxes: [{
				ticks: {
					beginAtZero: false,
					min: 0,
					max: 1
				}
			}]
		},
		elements: {
			line: {
				//lineTension: 0
			},
			point: {
				radius: 0
			}
		}
	}
});



var messages = [
]

$('#customer-input').keypress(function (e) {
	var message = $('#customer-input').val();
	if (e.which === 13) {
		messages.push({
			"type": "customer",
			"message": message
		})
		var payload = {
			'messages': messages 
		}
		$.ajax({
			url: "/api/v0/conversation/customer",
			data: JSON.stringify(payload),
			type: "POST",
			contentType: 'application/json',
		}).done(function (data) {
			render();
			updateSentiment(data.sentiment);
			updateEntities(data.entities);
		})
	}
})

$.ajax({
	url: "/api/v0/conversation/energy",
	type: "POST",
	contentType: 'application/json',
}).done(function (data) {
	updateEnergy(data);
	updateFrequency();
})

function render() {
	$('#area-chat').val("");
	var text = "";
	$.each(messages, function (k, v) {
		text += v.type + ": " + v.message + "\n";
	})
	$('#area-chat').val(text);
}

function updateEntities(entities) {
	$('#entities').empty();
	$.each(entities, function (k, v) {
		$("#entities").append("<li><span class='entity-label'>" + v.name + ": </span><a href='" + v.wikipediaUrl + "'>" + v.wikipediaId + "</a></li>");
	})

	$('#language').text("EN");
}

function updateFrequency() {
	frequencyChart.data.datasets.forEach((dataset) => {
		dataset.data = frequencyData;
	});
	frequencyChart.update()
}

function updateEnergy(energy) {
	var values = [];
	var labels = []
	var energyDeltas = []
	var deltas7 = []

	$.each(energy.energyTypical, function (k, v) {
		values.push(v);
		labels.push(k);
		energyDeltas.push(energy.energyCurrent[k] - v)
	});	

	//var steps = 14;
	//$.each(energyDeltas, function (k, v) {
	//	deltas7[k % steps] += v;
	//});
	//$.each(deltas7, function (k, v) {
	//	deltas7[k] = deltas7[k] / steps;
	//});

	//energyDeltaChart.data.labels = ["Mon", "Tue", "Wed", "Thu", "Fri", "Sat", "Sun"];
	//energyDeltaChart.data.labels = ["Mon1", "Mon2", "Tue1", "Tue2", "Wed1", "Wed2", "Thu1", "Thu2", "Fri1", "Fri2", "Sat1", "Sat2", "Sun1", "Sun2"];
	energyDeltaChart.data.labels = labels;
	energyDeltaChart.data.datasets[0].data = energyDeltas;
	energyDeltaChart.update();

	energyChart.data.labels = labels;
	energyChart.data.datasets[0].data = energy.energyCurrent;
	energyChart.update();

	energyAverageChart.data.labels = labels;
	energyAverageChart.data.datasets[0].data = energy.energyTypical;
	energyAverageChart.update();

	$('#vulrating').text(0.64).addClass("yellow");
	$('#supportLabel').text("Increasing").addClass("yellow");
	$('#energyLabel').text("Abnormal").addClass("yellow");
}

function updateSentiment(value) {
	var entries = sentimentData.slice();
	entries.push(value);

	//chart.data.labels.push(value);
	chart.data.datasets.forEach((dataset) => {
		dataset.data = entries;
	});
	chart.update();

}

$('#representative-input').keypress(function (e) {
	var message = $('#representative-input').val();
	if (e.which === 13) {
		messages.push({
			"type": "representative",
			"message": message
		})
		var payload = {
			'messages': messages
		}
		$.ajax({
			url: "/api/v0/conversation/customer",
			data: JSON.stringify(payload),
			type: "POST",
			contentType: 'application/json',
		}).done(function (data) {
			render();
			updateEntities(data.entities);
		})
	}
})

function index(data){
    //$.each(data, function(i, d){
    //    $('#index-content').append($('<li>').append(d));
    //});
}