<?php
/* DA SILVA Thomas, 06/03/18 : WebService Projet Lifi */

/* Lien vers fichier de connexion */
include_once("Connect.php");

/* Récupère le nombre total de places */
$nombre_ligne = $connect->query("SELECT COUNT(*) FROM place");

/* Requête principale */
$query = $connect->query("SELECT COUNT(*) FROM reservation WHERE Debut_Reservation < '" . $_GET["Fin_Reservation"] . "' AND Fin_Reservation > '" . $_GET["Debut_Reservation"] . "'");

$data = array();
while ($s = mysqli_fetch_assoc($nombre_ligne))
{
	while ($r = mysqli_fetch_assoc($query))
	{
	//$data = array(4-($r["COUNT(*)"]));
	$data = ($s["COUNT(*)"])-($r["COUNT(*)"]);
	}
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