<?php
/* DA SILVA Thomas, 06/03/18 : WebService Projet Lifi */

/* Lien vers fichier de connexion */
include_once("Connect.php");

$POST = json_decode(file_get_contents("php://input"), true);

/* Requête principale */
$query = "DELETE FROM reservation WHERE Reference = '" . $_GET["Reference"] . "';";

header("content-type: application/json");

if ($connect->query($query))
{
	echo "Delete OK";
}
else 
{
	echo "Erreur";
	http_response_code(400);
}

/* Ferme la connexion */
@mysqli_close($connect);
?>