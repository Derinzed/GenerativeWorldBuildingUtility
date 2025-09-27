using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeWorldBuildingUtility.Model
{
    public class AIModel
    {
        public string ModelName { get; set; }
        public string SystemPrefix { get; set; } = "You are a random generator for D&D and like TTRPGs. Answer the following questions like you were writing a D&D module, but be creative and unique.";
        //Controls the randomness of the output. Values range from 0.0 to 2.0. Lower values make the output more focused and deterministic, while higher values introduce more randomness and creativity. 
        public float Temperature { get; set; } = 1.0f;
        //Implements nucleus sampling, considering only the tokens comprising the top 'p' probability mass. Values range from 0.0 to 1.0. Lower values make the output more focused, while higher values allow for more diversity. 
        public float Top_p { get; set; } = 0;
        //A number between -2.0 and 2.0. Positive values penalize new tokens based on whether they appear in the text so far, increasing the model's likelihood to talk about new topics
        public float presence_penalty { get; set; } = 0;
        //A number between -2.0 and 2.0. Positive values penalize new tokens based on their existing frequency in the text so far, reducing the model's tendency to repeat the same line verbatim. ​
        public float frequency_penalty { get; set; } = 0;

    }
}
