<?php
	// Configuration
	$hostname = 'db';
	$username = 'php-add-score';
	$password = 'php-add-score-password';
	$database = 'highscores';
	
	$secretKey = "mySecretKey"; //Used for authentication of the received data. Has to match with the value stored in the game-client.

	function return500() {
		http_response_code(500);
		die();
	}
	
	function exceptionHandler($exception) {
		return500();
		//echo "<b>Exception:</b> ", $exception->getMessage();
	}
	
	set_exception_handler("exceptionHandler");

	try {
		$dbConnection = new PDO('mysql:dbname=' . $database . ';host=' . $hostname . ';charset=utf8', $username, $password);
		$dbConnection->setAttribute(PDO::ATTR_EMULATE_PREPARES, false);
		$dbConnection->setAttribute(PDO::ATTR_ERRMODE, PDO::ERRMODE_EXCEPTION);
	} catch(PDOException $e) {
		return500();
	}
	
	$name = $_GET['name'];
	$score = $_GET['score'];
	$level = $_GET['level'];
	$challengename = $_GET['challengename'];
	$hash = $_GET['hash'];
	
	if (is_null($challengename)) {
		$challengename = "";
	}

	$realHash = hash('sha256', $name . $score . $level . $challengename . $secretKey);

	if ($realHash == $hash) {
		$sqlQuery = $dbConnection->prepare('INSERT INTO scores (id, name, score, level, challengename, timestamp) VALUES (NULL, :name, :score, :level, :challengename, DEFAULT)');
		try {
			$sqlQuery->execute(['name' => $name, 'score' => $score, 'level' => $level, 'challengename' => $challengename]);
			$id = $dbConnection->lastInsertId();
			header('Content-type:application/json;charset=utf-8');
			echo json_encode(['id' => $id]);
		} catch(Exception $e) {
			return500();
		}
	} else {
		return500();
	}
?>