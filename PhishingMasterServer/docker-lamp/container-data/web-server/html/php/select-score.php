<?php
	// Configuration
	$hostname = 'db';
	$username = 'php-select-score';
	$password = 'php-select-score-password';
	$database = 'highscores';
	
	function return500() {
		http_response_code(500);
		die();
	}
	
	function exceptionHandler($exception) {
		return500();
		//echo "<b>Exception:</b> ", $exception->getMessage();
	}
	
	function addRowToResult($row,$result) {
		array_push($result,['name' => $row['name'],'score' => $row['score'],'level' => $row['level'],'challengename' => $row['challengename'],'timestamp' => $row['timestamp']]);
		return $result;
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
	$fromPos = $_GET['fromPos'];
	$count = $_GET['count'];
	$level = $_GET['level'];
	$order = $_GET['order'];
	$id = $_GET['id'];
	
	$selectPrefix = "SELECT name,score,level,challengename,timestamp FROM scores";
	if (!is_null($order) and $order == 'time') {
		$orderString = "ORDER BY timestamp desc";
	} else {
		$orderString = "ORDER BY score desc, timestamp desc";
	}
	
	$result = [];
	if (!is_null($id)) {
		//requesting data for specific id
		$sqlQuery = $dbConnection->prepare($selectPrefix . " WHERE id = :id ");
		try {
			$sqlQuery->execute(['id' => $id]);
			while($row = $sqlQuery->fetch()) {
				$result = addRowToResult($row,$result);
			}
		} catch(Exception $e) {
			return500();
		}
	} else if (!is_null($name)) {
		//requesting data for names like the given name
		$sqlQuery = $dbConnection->prepare($selectPrefix . " WHERE level = :level AND name LIKE :name " . $orderString);
		try {
			$sqlQuery->execute(['level' => $level, 'name' => $name]);
			while($row = $sqlQuery->fetch()) {
				$result = addRowToResult($row,$result);
			}
		} catch(Exception $e) {
			return500();
		}
	} else if (!is_null($count)) {
		if (is_null($fromPos)) {
			$fromPos = 0;
		}
		$sqlQuery = $dbConnection->prepare($selectPrefix . " WHERE level = :level AND name != 'Anonym' " . $orderString . " LIMIT :count OFFSET :fromPos");
		try {
			$sqlQuery->execute(['level' => $level, 'count' => $count, 'fromPos' => $fromPos]);
			while($row = $sqlQuery->fetch()) {
				$result = addRowToResult($row,$result);
			}
		} catch(Exception $e) {
			return500();
		}
	} else {
		$sqlQuery = $dbConnection->prepare($selectPrefix . " WHERE level = :level AND name != 'Anonym' " . $orderString);
		try {
			$sqlQuery->execute(['level' => $level]);
			while($row = $sqlQuery->fetch()) {
				$result = addRowToResult($row,$result);
			}
		} catch(Exception $e) {
			return500();
		}
	}
	
	
	header('Content-type:application/json;charset=utf-8');
	echo json_encode($result);
?>