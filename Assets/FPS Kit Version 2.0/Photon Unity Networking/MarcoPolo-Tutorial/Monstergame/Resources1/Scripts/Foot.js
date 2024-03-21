var baseFootAudioVolume = 1.0;
var soundEffectPitchRandomness = 0.05;

function OnTriggerEnter (other : Collider) {
	var collisionParticleEffect : CollisionParticleEffect = other.GetComponent(CollisionParticleEffect);
	
	if (collisionParticleEffect) {
		Instantiate(collisionParticleEffect.effect, transform.position, transform.rotation);
	}
	
	var collisionSoundEffect : CollisionSoundEffect = other.GetComponent(CollisionSoundEffect);

	if (collisionSoundEffect) {
		GetComponent.<AudioSource>().clip = collisionSoundEffect.audioClip;
		GetComponent.<AudioSource>().volume = collisionSoundEffect.volumeModifier * baseFootAudioVolume;
		GetComponent.<AudioSource>().pitch = Random.Range(1.0 - soundEffectPitchRandomness, 1.0 + soundEffectPitchRandomness);
		GetComponent.<AudioSource>().Play();		
	}
}

function Reset() {
	GetComponent.<Rigidbody>().isKinematic = true;
	GetComponent.<Collider>().isTrigger = true;
}

@script RequireComponent(AudioSource, SphereCollider, Rigidbody)
