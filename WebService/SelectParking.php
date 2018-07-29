<?php
/* DA SILVA Thomas, 06/03/18 : WebService Projet Lifi */

/* Lien vers fichier de connexion */
include_once("Connect.php");

/* Requête principale */
$query = $connect->query("SELECT ID, Nom, Adresse, Code_Postal, Ville FROM parking");

$data = array();
while ($r = mysqli_fetch_assoc($query))
{
	$data[] = array("ID" => $r["ID"], "Nom" => $r["Nom"], "Adresse" => $r["Adresse"], "Code_Postal" => $r["Code_Postal"], "Ville" => $r["Ville"]);
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