<?php
/* DA SILVA Thomas, 06/03/18 : WebService Projet Lifi */

/* Lien vers fichier de connexion */
include_once("Connect.php");

$query = $connect->query("SELECT ID, Nom, Prenom, Num_Tel, Adresse, Ville, Code_Postal, Mail, Mot_de_Passe FROM client WHERE Mail = '" . $_GET["Mail"] . "' AND Mot_de_Passe = '" . $_GET["Mot_de_Passe"] . "'");

$data = array();
while ($r = mysqli_fetch_assoc($query))
{
	$data[] = array("ID" => $r["ID"], "Nom" => $r["Nom"], "Prenom" => $r["Prenom"], "Num_Tel" => $r["Num_Tel"], "Adresse" => $r["Adresse"], "Ville" => $r["Ville"], "Code_Postal" => $r["Code_Postal"], "Mail" => $r["Mail"], "Mot_de_Passe" => $r["Mot_de_Passe"]);
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