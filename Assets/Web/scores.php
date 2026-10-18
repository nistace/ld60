<?php

ini_set('display_errors', '1');
error_reporting(E_ALL);

$config = __DIR__ . '/../../../config/db.php';
require_once $config;

$conn = new mysqli(DB_HOST, DB_USER, DB_PASS, DB_NAME);
if ($conn->connect_error) die("failed connection: " . $conn->connect_error);

if ($_SERVER['REQUEST_METHOD'] === 'GET') {
    $res = $conn->query("SELECT level, user_name, time FROM ( SELECT level, user_name, time, ROW_NUMBER() OVER (PARTITION BY level ORDER BY time ASC, user_name ASC ) AS rn FROM ld60_scores) ranked WHERE rn = 1 ORDER BY level");

    $out = [];
    $out["scores"] = $res->fetch_all(MYSQLI_ASSOC);

    header('Content-type: application/json');
    echo(json_encode($out));
    $conn->close();
    return http_response_code(200);
}

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $json = file_get_contents('php://input');
    $data = json_decode($json);
    
    $level = $data->level;
    $name = $data->user_name;
    $time = $data->time;
    
    // Get current best time
    $statement = $conn->prepare(
        "SELECT MIN(time) FROM ld60_scores WHERE level = ?"
    );
    $statement->bind_param("s", $level);
    $statement->execute();
    $statement->bind_result($bestTime);
    $statement->fetch();
    $statement->close();
    
    // Check if this is a new record
    $isNewRecord = $bestTime === null || $time < $bestTime;

    $statement = $conn->prepare("insert into ld60_scores (level, user_name, time) values (?, ?, ?)");
    $statement->bind_param("ssi", $level, $name, $time);

    $statement->execute();

    $out = [];
    $out["success"] = true;
    $out["id"] = $conn->insert_id;
    $out["best"] = $isNewRecord;

    header('Content-type: application/json');
    echo(json_encode($out));
    $conn->close();
    return http_response_code(200);
}

$conn->close();
return http_response_code(400);

