<?php
/* DA SILVA Thomas, 06/03/18 : WebService Projet Lifi */

/* Lien vers fichier de connexion */
include_once("Connect.php");

/* Requête principale */
$query = $connect->query("SELECT etage.Numero AS Num, allee.Lettre, place.Numero FROM etage, allee, place, reservation WHERE etage.ID = allee.Etage_ID AND allee.ID = place.Allee_ID AND reservation.Place_ID = place.ID AND reservation.Reference = '" . $_GET["Reference"] . "';");

$data = array();
while ($r = mysqli_fetch_assoc($query))
{
	$data[] = array("Etage" => $r["Num"], "Allee" => $r["Lettre"], "Numero" => $r["Numero"]);
}

header("content-type: application/json");
if (empty($data))
{
	echo "Aucun résultat"; 
	http_response_code(400);
} 

else
{
	echo json_encode($data);
}

/* Ferme la connexion */
@mysqli_close($connect);
?>