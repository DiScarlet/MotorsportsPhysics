using Microsoft.AspNetCore.Components;

namespace MotorsportsPhysics.Components.Pages
{
    public partial class MaxiDifficulty : ComponentBase
    {
        [Parameter]
        public EventCallback OnDialogueComplete { get; set; }

        private int index = 0;

        // Dialogue lines
        private readonly string[] dialogue = new[]
        {
            "Hey, Engineer! What's our run plan for today?",
            "I know all this stuff is overwhelming, but let me explain.",

            "Easy’s basically our FP1. Nice and steady, no pressure, no rivals.",
            "I get one question at a time, and you can take all the time you need.",
            "If we want to understand the physics without the stress… this is the place.",

            "Now Medium one’s more serious. We'll have five rival cars on track.",
            "You’ll still get explanations, and there’s no timer… ",
            "Think of it like qualifying: we’re pushing, but still breathing.",

            "And the Hard... this is where the real fun begins.",
            "Rivals? Five. Competition? Full send. Timer? Yeah, we’re on the clock.",
            "No explanations — just you, the physics, and your race instincts.",
            "If we miss question n by lap n, the car halts while everyone else keeps going.",
            "Feels just like Sunday — no mistakes allowed.",

            "So, engineer… what are we going for? Practise, qualifying, or the Grand Prix?"
        };

        // Properties for the .razor file
        public string CurrentSprite =>
            index < 1 ? "/images/Maxie/Maxie-greet.png" :
            index == 1 ? "/images/Maxie/Maxie-neutral.png" :
            index <= 4 ? "/images/Maxie/Maxie-easy.jpeg" :
            index < 8 ? "/images/Maxie/Maxie-medium.png" :
            index < 12 ? "/images/Maxie/Maxie-hard.jpeg" :
            "/images/Maxie/Maxie-neutral.png";

        public string CurrentDialogue => dialogue[index];

        public bool ShowContinueButton => index < dialogue.Length - 1;

        public bool DialogueComplete => index == dialogue.Length - 1;

        public async Task NextDialogue()
        {
            if (index < dialogue.Length - 1)
            {
                index++;
                if (index == dialogue.Length - 1)
                {
                    await OnDialogueComplete.InvokeAsync();
                }
                StateHasChanged();
            }
        }
    }
}
