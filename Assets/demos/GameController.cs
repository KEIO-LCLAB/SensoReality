using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace demos
{
    public class GameController : MonoBehaviour
    {
        public enum Action
        {
            Rock,
            Paper,
            Scissors,
            Nil
        }
        
        [SerializeField] private Image _rock;
        [SerializeField] private Image _paper;
        [SerializeField] private Image _scissors;
        [SerializeField] private TextMeshProUGUI _resultText;

        public void SetAction(Action action)
        {
            switch (action)
            {
                case Action.Rock:
                    _rock.enabled = true;
                    _paper.enabled = false;
                    _scissors.enabled = false;
                    _resultText.text = "Rock";
                    break;
                case Action.Paper:
                    _rock.enabled = false;
                    _paper.enabled = true;
                    _scissors.enabled = false;
                    _resultText.text = "Paper";
                    break;
                case Action.Scissors:
                    _rock.enabled = false;
                    _paper.enabled = false;
                    _scissors.enabled = true;
                    _resultText.text = "Scissors";
                    break;
                case Action.Nil:
                    _rock.enabled = false;
                    _paper.enabled = false;
                    _scissors.enabled = false;
                    _resultText.text = "";
                    break;
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetAction(Action.Rock);
            if (Input.GetKeyDown(KeyCode.Alpha2)) SetAction(Action.Paper);
            if (Input.GetKeyDown(KeyCode.Alpha3)) SetAction(Action.Scissors);
            if (Input.GetKeyDown(KeyCode.Alpha4)) SetAction(Action.Nil);
        }
    }
}
