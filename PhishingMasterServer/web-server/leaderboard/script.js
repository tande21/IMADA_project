// Address used to contact highscore server.
// Use "" to use the same address where this page is hosted.
// Addresses will be used in array order, if the first address does not work.
var highscoreServerAddresses = ["../", "http://phishing-master-highscore-server.local/", "http://localhost/"];
var highscoreLevel = "fake_company_level_1"; //Level with real company names

var players;
var backgroundReloadRun;

document.addEventListener('DOMContentLoaded', () => {
	var url_string = window.location.href
    var url = new URL(url_string);
    var returnStr = url.searchParams.get("level");
	if (returnStr != null) {
		highscoreLevel = returnStr;
	}
	clearPlayers();
	reloadLeaderBoard("leaderboard1");
	reloadLeaderBoard("leaderboard2");
	setTimeout(function(){backgroundReloadRun = setInterval(backgroundReload, 3000); }, 2000);
});

function backgroundReload() {
	console.log("reload");
	httpGetAsync(highscoreServerAddresses, "leaderboard1", getUrl("leaderboard1"), sortInResponse, handleServerError);
	httpGetAsync(highscoreServerAddresses, "leaderboard2", getUrl("leaderboard2"), sortInResponse, handleServerError);
}

function reloadLeaderBoard(leaderboardName) {
	for (var i = 0; i < players.length; i++) {
		players[i] = {name: "Loading...", score: 0};
	}
	drawLeaderBoard(leaderboardName);
	
	//fetch data from server
	httpGetAsync(highscoreServerAddresses, leaderboardName, getUrl(leaderboardName), sortInResponse, handleServerError);
}

function getUrl(leaderboardName) {
	if(leaderboardName == "leaderboard1") {
		return "php/select-score.php?level=" + highscoreLevel;
	} else if (leaderboardName == "leaderboard2"){
		return "php/select-score.php?level=" + highscoreLevel + "&order=time";
	}
}

function sortInResponse(leaderboardName, responseText) {
	clearPlayers();
	drawLeaderBoard(leaderboardName);
	players = JSON.parse(responseText);
	drawLeaderBoard(leaderboardName);
}

function handleServerError(leaderboardName) {
	//try a local server:
	console.log("error:"+statusText);
	clearPlayers();
	players[0] = {name: "Connection failed!", score: 0};
	drawLeaderBoard(leaderboardName);
}

function drawLeaderBoard(leaderboardName) {
	var leaderboard = document.querySelector('#' + leaderboardName);
	var names = leaderboard.querySelectorAll('.name');
	var scores = leaderboard.querySelectorAll('.number');
	var i;
	for (i = 0; i < names.length; i++) {
		if(players[i]) {
			names[i].innerHTML = players[i].name;
			scores[i].innerHTML = numberWithCommas(players[i].score);
		}
	}
}

function httpGetAsync(addresses, leaderboardName, url, callback, errorCallback) {
	var xhr;
	if (addresses.length > 0) {
		xhr = new XMLHttpRequest();
		xhr.open("GET", addresses[0] + url, true);
		xhr.onload = function (e) {
			if (xhr.readyState == 4) {
				if (xhr.status == 200) {
					callback(leaderboardName, xhr.responseText);
				} else {
					httpGetAsync(addresses.slice(1), leaderboardName, url, callback, errorCallback);
				}
			}
		}
		xhr.onerror = function (e) {
			httpGetAsync(addresses.slice(1), leaderboardName, url, callback, errorCallback);
		}
		xhr.send(null);
	} else {
		errorCallback(leaderboardName);
	}
}

function clearPlayers() {
	players = [
	{name: "", score: 0},
	{name: "", score: 0},
	{name: "", score: 0},
	{name: "", score: 0},
	{name: "", score: 0},
	{name: "", score: 0},
	{name: "", score: 0},
	{name: "", score: 0},
	{name: "", score: 0},
	{name: "", score: 0}
	];
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
}