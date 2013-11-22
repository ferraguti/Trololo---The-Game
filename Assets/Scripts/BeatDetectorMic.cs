	using UnityEngine;
using System.Collections;

public class BeatDetectorMic : MonoBehaviour 
{
	public Transform debugSpectrePrfb; //Prefab pour l'objet de debug du spectre
	public Transform debugBeatPrfb; //Prefab pour l'objet de debug de la detection de beat
	public int debutAnalyseSpectre = 0; //
	public int finAnalyseSpectre = BUFFSPECTRESIZE/8; //
	public float sensibVarSign = 1.3f; //
	public float seuilEnergieVariance = 0.01f;
	
	private const int SPECTRESIZE = 1024; //Taille du tableau de coeffs de fourier
	private float[] spectre; //Le spectre de la dernière analyse
	private const float ANALYSEDURATION = 2.0f; //Durée du buffer tournant pour l'analyse du spectre (en secondes)
	private const float ANALYSEFREQ = 120.0f; //Frequence d'analyse (par seconde)
	private const float ANALYSEPERIOD = 1.0f/ANALYSEFREQ; //Durée d'une analyse
	private const int BUFFSPECTRESIZE = (int)(ANALYSEFREQ * ANALYSEDURATION); //Taille du buffer tournant pour l'analyse du spectre
	private float [] buffSpectre; //Buffer tournant 
	private int posBufSpectre; //Position dans le buffer tournant
	private double timeLastAnalyse; //Timecode de la dernière analyse
	private float energieMoyenne = 0; //Energie moyenne sur la durée du buffer
	private float energieVariance = 0; //Variance energie sur la durée du buffer
		
	
	private Transform [] objSpecDebug; //Pour le debug, rendu du spectre
	private Transform objBeatDebug; //Pour le debug, rendu du beat
	
	private bool MicIsOk = false;

    Player player;
 
	
	IEnumerator InitMic(){
		
		yield return Application.RequestUserAuthorization(UserAuthorization.Microphone);
		
		if (Application.HasUserAuthorization(UserAuthorization.Microphone))
	    {
	        //Reservation mémoire
			spectre = new float[SPECTRESIZE];
			for(int i=0;i<SPECTRESIZE;i++) 
				spectre[i] = 0; //Init Ã  0
			
			//On crée les objets de debug
			if(debugSpectrePrfb != null) {
				//On récupÃ¨re la taille du mesh
				Mesh mesh = debugSpectrePrfb.GetComponent<MeshFilter>().sharedMesh;
					Vector3 sizeMesh = new Vector3(mesh.bounds.size.x,mesh.bounds.size.y,mesh.bounds.size.z);
				
				//On créer le tableau d'objets pour rendre le spectre
				objSpecDebug = new Transform[SPECTRESIZE];
				for(int i=0;i<SPECTRESIZE;i++)
					objSpecDebug[i] = (Transform)Instantiate(debugSpectrePrfb, new Vector3(transform.position.x, 0,transform.position.z+i*(sizeMesh.z+1)), Quaternion.identity); 		
			}
			else 
				objSpecDebug = null;
			
			//On crée l'objet de debug pour le beat
			if(debugBeatPrfb != null) {
				//On récupère la taille du mesh
				Mesh mesh = debugBeatPrfb.GetComponent<MeshFilter>().sharedMesh;
				Vector3 sizeMesh = new Vector3(mesh.bounds.size.x,mesh.bounds.size.y,mesh.bounds.size.z);
				
				//On crée l'objet de debug du beats
				objBeatDebug = (Transform)Instantiate(debugBeatPrfb, new Vector3(transform.position.x + sizeMesh.x + 1, 0,transform.position.z), Quaternion.identity); 
				
				
			}
			
			//Creation du buffer pour l'analyse du specre
			buffSpectre = new float[BUFFSPECTRESIZE];
			for(int i=0;i<BUFFSPECTRESIZE;i++) buffSpectre[i] = 0; //Init Ã  0
			posBufSpectre = 0;
			
			MicIsOk = true;
	    }
	    else
	    {
	        MicIsOk = false;
	    }		
    }
		
	//Init de l'objet
	void  Start () {
		
		StartCoroutine(InitMic());
		
		player = GameObject.FindObjectOfType(typeof(Player)) as Player;

        if (player.input != INPUT.MIKE)
            Destroy(this);
	}
	
	// Update a chaque frame
	void Update () {
		audio.volume = 0f;
		
		if(!MicIsOk)
			return;
		
		//Si on a atteint la fin du buffer audio ou si on l'a jamais créé
		if (!audio.isPlaying) {
			//On se met trois minutes de buffer sur le microphone
	      	audio.clip = Microphone.Start("Built-in Microphone", true, 180, 44100);
			
			//On attend qu'il y'a eu un peu d'acquisition
			while (!(Microphone.GetPosition("Built-in Microphone") > 500)) { } 
			
			//On lance la lecture
			audio.Play();
			
			//On init l'analyse
			timeLastAnalyse = 0;
		}
			
		//On récupÃ¨re le spectre
		audio.GetSpectrumData(spectre, 0, FFTWindow.BlackmanHarris);
		
		//Si on debug, on affiche
		for (var i = 1; i < SPECTRESIZE-1; i++) {
			Vector3 point1 = new Vector3 (i - 1, spectre[i]*100, 0) + transform.position;
			Vector3 point2 = new Vector3 (i, spectre[i + 1]*100, 0) + transform.position;
        	Debug.DrawLine (point1, point2, Color.red);
    	}
		
		//Si c'est le moment de faire une nouvelle analyse
		double timeSinceLastAnalyse = audio.time - timeLastAnalyse;
		if(timeSinceLastAnalyse > ANALYSEPERIOD){
			//On se replace au bon endroit
			timeLastAnalyse = audio.time;
						
			//On fait la somme des fréquences, dans la frange choisie
			float energie = 0;
			for(int i=debutAnalyseSpectre;i<finAnalyseSpectre;i++)
				energie += spectre[i];
			
			//On seuille pour éviter les soucis
			if(energie < 0.0001) energie = 0;
			//Debug.Log("Energie " + energie);
			
			//Si on arrive pas a échantilloner Ã  la vitesse voulue, on stoque plusieurs fois la mesure
			int nbSlots =(int)(timeSinceLastAnalyse / ANALYSEPERIOD); 
			//Debug.Log("nbSlots " + nbSlots);
			
			//On ajoute au buffer tournant en mettant Ã  jour la moyenne en meme temps
			energieMoyenne *= BUFFSPECTRESIZE;
			for(int i=0;i<nbSlots;i++){
				posBufSpectre = (posBufSpectre+1) % BUFFSPECTRESIZE;
				energieMoyenne -= buffSpectre[posBufSpectre];
				buffSpectre[posBufSpectre] = energie;	
				energieMoyenne += energie;
			}
			energieMoyenne /= (float)BUFFSPECTRESIZE;
			
			//On seuille pour éviter les soucis
			if(energieMoyenne < 0.0001) energieMoyenne = 0;
			//Debug.Log("Mean " + energieMoyenne);
					
			//On met Ã  jour la variance de l'energie
			energieVariance = 0;
			for(int i=0;i<BUFFSPECTRESIZE;i++) {
				float ecart = energieMoyenne-buffSpectre[i];
				energieVariance += ecart * ecart;
			}
			energieVariance /= (float)BUFFSPECTRESIZE;
			energieVariance = Mathf.Sqrt(energieVariance); //A supprimer
			
			if(energieVariance < 0.0001) energieVariance = 0;
			//Debug.Log("Variance " + energieVariance);
			
			//Si l'energie actuelle dépasse n fois la variance du buffer actuel, alors on a un beat
			if(energieVariance > seuilEnergieVariance && energie > energieMoyenne + (energieVariance*sensibVarSign)) 
            {
				//Debug.Log(Time.frameCount);
				//float[] args = new float[]{audio.volume, audio.pitch, audio.time};

                player.CheckJump();
				
				
				if(objBeatDebug != null) {
					objBeatDebug.transform.position = new Vector3(objBeatDebug.transform.position.x, 10, objBeatDebug.transform.position.z);
				}
			}
		}
		
		//Si on a un objet de debug du beat
		if(objBeatDebug != null && objBeatDebug.transform.position.y > 0) {
			objBeatDebug.transform.Translate(new Vector3(0,-(Time.deltaTime*10),0));
		}

	}
	
	void Tick(){
	}
}
