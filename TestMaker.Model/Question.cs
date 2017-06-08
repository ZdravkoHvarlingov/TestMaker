using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestMaker.Model
{
    public class Question
    {
        public int QuestionId { get; set; }      

        public string QuestionText { get; set; }

        public string QuestionAnswerA { get; set; }

        public string QuestionAnswerB { get; set; }

        public string QuestionAnswerC { get; set; }

        public string QuestionAnswerD { get; set; }

        public string QuestionTrueAnswer { get; set; }

        public QuestionType QuestionType { get; set; }

        public QuestionField QuestionField { get; set; }
    }
}
