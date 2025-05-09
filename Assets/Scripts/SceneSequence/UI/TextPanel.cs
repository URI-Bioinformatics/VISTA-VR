using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class TextPanel : MonoBehaviour
{
    public enum PanelType { Instruction, Video, Question }

    public GameObject Panel;

    [Header("Instructions")]
    [SerializeField] private GameObject instructionCard;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private GameObject continueButtonObj;

    [Header("Video")]
    [SerializeField] private GameObject videoCard;
    [SerializeField] private VideoPlayerManager videoPlayerManager;
    [SerializeField] private TextMeshProUGUI contentTextVideo;

    [Header("Question")]
    [SerializeField] private GameObject questionCard;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private List<QuestionOption> options = new List<QuestionOption>(4);
    [SerializeField] private GameObject checkAnswerButton;
    private int selectedAnswerIndex = -1;
    private int correctAnswerIndex = -1;
    private bool answeredCorrectly = false;
    private int incorrectAttempts = 0;

    [Header("Extra Settings")]
    public float scaleDuration = 0.5f;

    private void OnEnable()
    {
        UIEvents.ScaleFloatingPanelTo.AddListener(ScalePanel);
        UIEvents.MoveFloatingPanelTo.AddListener(MovePanelPos);
        UIEvents.SetCardType.AddListener(SetCardActive);

        UIEvents.SetFloatingPanelTitle.AddListener(SetPanelTitle);
        UIEvents.SetFloatingInstructionText.AddListener(SetPanelContentString);
        UIEvents.SetActiveFloatingPanelContinueButton.AddListener(ToggleContinue);

        UIEvents.SetVideoClip.AddListener(SetVideoClip);
        UIEvents.PlayVideo.AddListener(PlayVideo);
        UIEvents.SetFloatingVideoText.AddListener(SetVideoText);

        UIEvents.SetFullQuestion.AddListener(SetQuestion);
    }

    private void OnDisable()
    {
        UIEvents.ScaleFloatingPanelTo.RemoveListener(ScalePanel);
        UIEvents.MoveFloatingPanelTo.RemoveListener(MovePanelPos);
        UIEvents.SetCardType.RemoveListener(SetCardActive);

        UIEvents.SetFloatingPanelTitle.RemoveListener(SetPanelTitle);
        UIEvents.SetFloatingInstructionText.RemoveListener(SetPanelContentString);
        UIEvents.SetActiveFloatingPanelContinueButton.RemoveListener(ToggleContinue);
        
        UIEvents.SetVideoClip.RemoveListener(SetVideoClip);
        UIEvents.PlayVideo.RemoveListener(PlayVideo);
        UIEvents.SetFloatingVideoText.RemoveListener(SetVideoText);

        UIEvents.SetFullQuestion.RemoveListener(SetQuestion);
    }

    public void PressContinue()
    {
        UIEvents.FloatingPanelContinueButtonPressed.Invoke();
        Debug.Log($"Continue button active");
    }

    public void SetCardActive(PanelType panelType)
    {
        instructionCard.SetActive(panelType == PanelType.Instruction);
        videoCard.SetActive(panelType == PanelType.Video);
        questionCard.SetActive(panelType == PanelType.Question);
        Debug.Log($"Set card active: {panelType}"); 
    }

    private void ToggleContinue(bool value)
    {
        continueButtonObj?.SetActive(value);
    }

    private void SetPanelTitle(string newTitle)
    {
        if (titleText != null)
            titleText.text = newTitle;
    }

    private void SetPanelContentString(string newContent)
    {
        if (contentText != null)
            contentText.text = newContent;
    }

    private void SetVideoText(string newContent)
    {
        if (contentText != null)
            contentTextVideo.text = newContent;
    }

    public void SetQuestion(string questionTextValue, string[] optionTexts, int correctIndex)
    {
        questionText.text = questionTextValue;
        selectedAnswerIndex = -1;
        correctAnswerIndex = correctIndex;
        answeredCorrectly = false;
        incorrectAttempts = 0;

        for (int i = 0; i < options.Count; i++)
        {
            int index = i;
            var option = options[i];
            var button = option.button;
            var image = button.GetComponent<Image>();
            var label = option.label;

            label.text = optionTexts[i];
            image.color = Color.white;

            button.interactable = true;
            button.onClick.RemoveAllListeners();

            var defaultColors = button.colors;
            defaultColors.normalColor = Color.white;
            defaultColors.highlightedColor = new Color(0.9f, 0.9f, 0.9f);
            defaultColors.pressedColor = new Color(0.75f, 0.75f, 0.75f);
            button.colors = defaultColors;

            button.onClick.AddListener(() =>
            {
                if (answeredCorrectly) return;

                selectedAnswerIndex = index;

                for (int j = 0; j < options.Count; j++)
                {
                    SetButtonColor(options[j].button, j == selectedAnswerIndex ? new Color(0.7f, 0.85f, 1f) : Color.white);
                }
            });
        }

        checkAnswerButton.SetActive(true);
        checkAnswerButton.GetComponent<Button>().onClick.RemoveAllListeners();
        checkAnswerButton.GetComponent<Button>().onClick.AddListener(CheckAnswer);
    }

    private void CheckAnswer()
    {
        if (answeredCorrectly || selectedAnswerIndex == -1) return;

        if (selectedAnswerIndex == correctAnswerIndex)
        {
            Debug.Log("Correct answer selected.");
            SetButtonColor(options[selectedAnswerIndex].button, Color.green);

            foreach (var opt in options)
                opt.button.interactable = false;

            checkAnswerButton.SetActive(false);
            answeredCorrectly = true;
            UIEvents.QuestionComplete.Invoke();
        }
        else
        {
            incorrectAttempts++;
            Debug.Log($"Incorrect answer. Attempts: {incorrectAttempts}");
            SetButtonColor(options[selectedAnswerIndex].button, Color.red);

            selectedAnswerIndex = -1; // Require reselection
        }
    }

    private void SetButtonColor(Button btn, Color color)
    {
        var cb = btn.colors;
        cb.normalColor = color;
        cb.highlightedColor = color;
        cb.pressedColor = color * 0.9f;
        cb.disabledColor = color;
        btn.colors = cb;
    }

    public void PanelLeave()
    {
        StartCoroutine(ScaleOverTime(Panel, Vector3.zero, scaleDuration));
    }

    private IEnumerator ScaleOverTime(GameObject obj, Vector3 targetScale, float duration)
    {
        Vector3 initialScale = obj.transform.localScale;
        float time = 0f;

        while (time < duration)
        {
            obj.transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        obj.transform.localScale = targetScale;
    }

    private void MovePanelPos(Vector3 newPos)
    {
        Panel.transform.localPosition = newPos;
    }

    private void ScalePanel(float newScale)
    {
        Panel.transform.localScale = Vector3.one * newScale;
    }

    public void SetVideoClip(VideoClip clip)
    {
        videoPlayerManager.SetVideoClip(clip);
    }

    public void PlayVideo(bool play)
    {
        videoPlayerManager.PlayVideo(play);
    }
}
