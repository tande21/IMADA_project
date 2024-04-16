var TEXT_CHALLENGE_MODE = "Herausforderungs-Modus";
var TEXT_NORMAL_MODE = "Normaler Modus";
var TEXT_NUMBER_ENTRIES = "Einträge";
var TEXT_LOADING = "Lade...";
// Address used to contact highscore server.
// Use "" to use the same address where this page is hosted.
// Addresses will be used in array order, if the first address does not work.
var highscoreServerAddresses = ["", "http://phishing-master-highscore-server.local/", "http://localhost/"];
var DEFAULT_HIGHSCORE_LEVEL = "fake_company_level_1"; //Web level with fake company names
var highscoreLevel = DEFAULT_HIGHSCORE_LEVEL;
var challengeModeLevel = "";
var challengeName = "";
var challengeMode = false;

var unityInstance;

var loadMore = false;

var players;

var challengeCodePopup;

document.addEventListener('DOMContentLoaded', () => {
	if (!checkChallengeCode()) {
		clearAndReload();
	}
	
	challengeCodePopup = document.getElementById("challengeCodePopup");
	document.getElementById("challengeCodePopupCloseButton").onclick = function() {
		challengeCodePopup.style.display = "none";
	};
});

function clearAndReload() {
	clearPlayers();
	reloadLeaderBoard();
}

//Function called by Unity. Don't change function name
function UnityDisplayChallengeCode(challengeCode, score) {
	unityInstance.SetFullscreen(0);
	challengeCodePopup.style.display = "block";
	var challengeCodeText = document.getElementById("challengeCodeText");
	var myUrl = window.location.protocol + "//" + window.location.hostname + window.location.pathname;
	var displayText = "Hi,\n"
					+ "ich habe im Spiel Phishing Master " + score + " Punkte erreicht.\n"
					+ "Du kannst versuchen meine Punktzahl zu schlagen, indem du auf folgenden Link klickst:\n"
					+ "\n"
					+ myUrl + "?c=" + challengeCode;
	challengeCodeText.value = displayText;
}

function CopyChallengeCodeTextToClipboard() {
	var challengeCodeText = document.getElementById("challengeCodeText");
	challengeCodeText.select();
	challengeCodeText.setSelectionRange(0, 99999)
	document.execCommand("copy");
	//unselect text
	if (window.getSelection) {
		window.getSelection().removeAllRanges();
	} else if (document.selection) {
		document.selection.empty();
	}
	//copy button feedback
	var cButton = document.getElementById("CopyChallengeCodeTextToClipboardButton");
	var cButtonText = cButton.querySelector("div");
	cButtonText.textContent = "Kopiert.";
	//cButton.style.color = "#4664aa";
	cButton.querySelector(".material-icons").textContent = "done";
	setTimeout(function(){
		cButtonText.textContent = "Kopieren";
		//cButton.style.color = "#000000";
		cButton.querySelector(".material-icons").textContent = "content_copy";
		},1000);
}

function checkChallengeCode() {
	var url_string = window.location.href
    var url = new URL(url_string);
    var returnStr = url.searchParams.get("c");
	if(returnStr == null) {
		return false;
	}
	//old challengeCode
	if (returnStr.split("-").length == 4) {
		challengeModeLevel = returnStr.split("-")[0] + "-" + returnStr.split("-")[1];
		challengeMode = false;
		changeLeaderboardMode();
		return true;
	}
	
	//new challengeCode
	var databaseEntryID = parseInt("0x" + returnStr.split("-")[0]);
	console.log(databaseEntryID);
	httpGetAsync(highscoreServerAddresses, "php/select-score.php?id=" + databaseEntryID, function(responseText) {
		try {
		challengeModeLevel = JSON.parse(responseText)[0].level;
		challengeName = JSON.parse(responseText)[0].challengename;
		} catch {}
		challengeMode = false;
		changeLeaderboardMode();
		}, function() {
		challengeMode = false;
		changeLeaderboardMode();
		});
	return true;
}

function changeLeaderboardMode() {
	challengeMode = !challengeMode;
	var myButton = document.getElementById("leaderboardModeButton");
	var myButtonText = myButton.querySelector("div");
	myButton.style.display = "inline-block";
	if (challengeMode) {
		highscoreLevel = challengeModeLevel;
		myButtonText.textContent = "⚔️ " + TEXT_CHALLENGE_MODE + ": " + challengeName + " ⚔️";
	} else {
		highscoreLevel = DEFAULT_HIGHSCORE_LEVEL;
		myButtonText.textContent = TEXT_NORMAL_MODE;
	}
	clearAndReload();
}

function loadGame() {
	unityInstance = UnityLoader.instantiate("unityContainer", "Build/htdocs.json", {onProgress: UnityProgress});
	document.getElementById("fullscreenButton").style.display = "inline-block";
	document.getElementById("fullscreenText").style.display = "inline-block";
}

function fullscreenButtonClicked() {
	if(unityInstance != null) {
		unityInstance.SetFullscreen(1);
	}
}

//Function called by Unity. Don't change function name
function UnityPlayerNameSubmitted(name, score) {
	clearAndReload();
}

function reloadLeaderBoard() {
	for (var i = 0; i < players.length; i++) {
		players[i] = {name: TEXT_LOADING, score: 0};
	}
	drawLeaderBoard();
	
	//fetch data from server
	httpGetAsync(highscoreServerAddresses, "php/select-score.php?level=" + highscoreLevel, sortInResponse, handleServerError);
}

function sortInResponse(responseText) {
	clearPlayers();
	drawLeaderBoard();
	players = JSON.parse(responseText);
	drawLeaderBoard();
}

function handleServerError() {
	clearPlayers();
	players[0] = {name: "Connection failed!", score: 0};
	drawLeaderBoard();
}

function drawLeaderBoard() {
	var leaderboard = document.querySelector('#leaderboard');
	var names = leaderboard.querySelectorAll('.name');
	var scores = leaderboard.querySelectorAll('.number');
	var i;
	for (i = 0; i < names.length; i++) {
		if(players[i]) {
			names[i].textContent = players[i].name;
			scores[i].textContent = numberWithCommas(players[i].score);
		}
	}
	if(players[0] != null && players[0].name != TEXT_LOADING) {
		var entriesCountElement = document.querySelector('#leaderboardEntriesCount');
		entriesCountElement.textContent = players.length + " " + TEXT_NUMBER_ENTRIES;
	}
}

function loadMoreButtonClicked() {
	loadMore = true;
	onHighscoreScrolled();
	var loadMoreButton = document.querySelector('#loadMoreButtonContainer');
	loadMoreButton.style.display = "none";
}

window.onscroll = function() { onHighscoreScrolled();};
function onHighscoreScrolled() {
	if(!loadMore) {
		return;
	}
	var bottomScreenPosition = (window.pageYOffset + window.innerWidth);
	var documentHeight = document.documentElement.scrollHeight;
	//console.log("OnScroll:" + bottomScreenPosition + "/" + documentHeight);
	if (bottomScreenPosition > documentHeight - 250) {
		var leaderboard = document.querySelector('#leaderboard');
		var loadMoreButton = document.querySelector('#loadMoreButtonContainer');
		var names = leaderboard.querySelectorAll('.name');
		var difference = players.length - names.length;
		var numberOfNewEntries = Math.min(difference,30);
		var newNumber = names.length + 1;
		while(numberOfNewEntries > 0) {
			//console.log("adding element");
			var li = document.createElement("li");
			var spanListNum = document.createElement("span");
			spanListNum.setAttribute('class',"list_num");
			spanListNum.textContent = newNumber;
			var h2 = document.createElement("h2");
			var spanName = document.createElement("span");
			spanName.setAttribute('class',"name");
			var spanNumber = document.createElement("span");
			spanNumber.setAttribute('class',"number");
			h2.appendChild(spanName);
			h2.appendChild(spanNumber);
			li.appendChild(spanListNum);
			li.appendChild(h2);
			leaderboard.insertBefore(li, loadMoreButton);
			numberOfNewEntries--;
			newNumber++;
		}
		if (difference != 0) {
			drawLeaderBoard();
		}
	}
}

function httpGetAsync(addresses, url, callback, errorCallback) {
	var xhr;
	if (addresses.length > 0) {
		xhr = new XMLHttpRequest();
		xhr.open("GET", addresses[0] + url, true);
		xhr.onload = function (e) {
			if (xhr.readyState == 4) {
				if (xhr.status == 200) {
					callback(xhr.responseText);
				} else {
					httpGetAsync(addresses.slice(1), url, callback, errorCallback);
				}
			}
		}
		xhr.onerror = function (e) {
			httpGetAsync(addresses.slice(1), url, callback, errorCallback);
		}
		xhr.send(null);
	} else {
		errorCallback();
	}
}

function clearPlayers() {
	var leaderboard = document.querySelector('#leaderboard');
	var names = leaderboard.querySelectorAll('.name');
	players = [{name: "", score: 0}];
	for (var i = 0; i < names.length; i++) {
		players[i] = {name: "", score: 0};
	}
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ".");
}