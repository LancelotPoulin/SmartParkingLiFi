<?php
/* DA SILVA Thomas, 06/03/18 : WebService Projet Lifi */

/* Lien vers fichier de connexion */
include_once("Connect.php");

$POST = json_decode(file_get_contents("php://input"), true);

$reference = 0;
$exist = true;

while ($exist == true)
{
	$reference = rand(1000, 9999);
	$req = $connect->query("select count(*) from reservation where Reference =". $reference .";");
	while ($r = mysqli_fetch_assoc($req))
	{	
		if ($r["count(*)"] == 0)
		{
			$exist = false;
		}
	}
}

/* Requête principale */
$query = "INSERT INTO reservation VALUES (NULL, $reference, '" . $_GET["Debut_Reservation"] . "', '" . $_GET["Fin_Reservation"] . "', NULL, NULL, NULL, '" . $_GET["Client_ID"] . "');";

/*$query = "INSERT INTO reservation VALUES (NULL, $reference, '" . $POST["Debut_Reservation"] . "', '" . $POST["Fin_Reservation"] . "', NULL, NULL, NULL, '" . $POST["Client_ID"] . "');";*/
 
header("content-type: application/json");
 
if ($connect->query($query))
{
	echo $reference . "\n" . "Insert invité OK";
}
else 
{
	echo "Erreur";
	http_response_code(400);
}

/* Ferme la connexion */
@mysqli_close($connect);
?>