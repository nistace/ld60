<?php

if ($_SERVER['REQUEST_METHOD'] !== 'GET') {
    header('Allow: GET');
    http_response_code(405);
    exit;
}

require_once __DIR__ . '/../../../config/db.php';

header('Content-Type: application/json');

try {
    $conn = new mysqli(DB_HOST, DB_USER, DB_PASS, DB_NAME);
    if ($conn->connect_error) {
        throw new RuntimeException('Database connection failed');
    }

    $result = $conn->query('SELECT level, user_name, time FROM ld60_scores ORDER BY level, time ASC, user_name ASC');
    if (!$result) {
        throw new RuntimeException('Score query failed');
    }

    echo json_encode(['scores' => $result->fetch_all(MYSQLI_ASSOC)]);
    $conn->close();
} catch (Throwable $error) {
    http_response_code(500);
    echo json_encode(['error' => 'Unable to load leaderboard scores.']);
}
