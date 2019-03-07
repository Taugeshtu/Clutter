using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTimer: MonoBehaviour {
	[SerializeField] private float _delay = 10.0f;
	[SerializeField] private string _sceneToGo;
	
	private float _timer;
	
#region Implementation
	void Start() {
		_timer = Time.time + _delay;
	}
	
	void Update() {
		if( Time.time > _timer ) {
			if( string.IsNullOrEmpty( _sceneToGo ) ) {
				SceneManager.LoadScene( SceneManager.GetActiveScene().buildIndex + 1 );
			}
			else {
				SceneManager.LoadScene( _sceneToGo );
			}
		}
	}
#endregion
	
	
#region Public
#endregion
	
	
#region Private
#endregion
	
	
#region Temp
#endregion
}
