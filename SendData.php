<?php

//to connect, was changed due to personal information
$servername = "hostsite.com";
$username = "username";
$password = "password";
$dbname = "database_test";

//variables sent to server
$game_id = $_POST["game_id"];
$init_state = $_POST["init_state"];
$num_rows = $_POST["num_rows"];
$num_cols = $_POST["num_cols"];
$kernel = $_POST["kernel"];
$ker_rows = $_POST["ker_rows"];
$ker_cols = $_POST["ker_cols"];
$moves = $_POST["moves"];
$finished = $_POST["finished"]

//connect to database
$conn = new mysqli($servername, $username, $password, $dbname);

//check if we managed to connect
if($conn -> connect_error){
	die("Connection Failed : " . $conn->connect_error);
}

echo "connected successfuly";

//Insert move into the database
$sql = "INSERT INTO moves(game_id, init_state, kernel, moves, finished)
VALUES ('".$game_id."', '".$init_state."', '".$num_rows."', '".$num_cols."', '".$kernel."', '".$ker_rows."', '".$ker_cols."', '".$moves."', '".$finished."')";
if($conn->query($sql)===TRUE) {
	echo "move added";
}
else {
	echo "Error : ".$sql. " " . $conn->error;
}

$conn->close();

?>