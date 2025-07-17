using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MFPC.PlayerStats
{
    [RequireComponent(typeof(Animation))]
    public class DeathScreen : MonoBehaviour
    {
        private Animation _animation;

        #region MONO

        private void Awake() => _animation = this.GetComponent<Animation>();

        private void OnEnable() => PlayerHealth.PlayerDeath += OnDeath;
        private void OnDisable() => PlayerHealth.PlayerDeath -= OnDeath;

        #endregion
        
        #region CALLBACK

        private void OnDeath()
        {
            _animation.Play();

            StartCoroutine(RestartGame());
        }

        #endregion

        private IEnumerator RestartGame()
        {
            yield return new WaitForSeconds(5f);
            
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
