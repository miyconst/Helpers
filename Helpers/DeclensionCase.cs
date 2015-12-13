
namespace Helpers
{
    /// <summary>
    /// Падежи русского языка
    /// </summary>
    public enum DeclensionCase
    {
        /// <summary>
        /// Падеж не определен
        /// </summary>
        NotDefind = 0,

        /// <summary>
        /// Именительный падеж (Кто? Что?)
        /// </summary>
        Imenit = 1,

        /// <summary>
        /// Родительный падеж (Кого? Чего?)
        /// </summary>
        Rodit = 2,

        /// <summary>
        /// Дательный падеж (Кому? Чему?)
        /// </summary>
        Datel = 3,

        /// <summary>
        /// Винительный падеж (Кого? Что?)
        /// </summary>
        Vinit = 4,

        /// <summary>
        /// Творительный падеж (Кем? Чем?)
        /// </summary>
        Tvorit = 5,

        /// <summary>
        /// Предложный падеж (О ком? О чём?)
        /// </summary>
        Predl = 6
    }

    /// <summary>
    /// Род
    /// </summary>
    public enum Gender
    {
        /// <summary>
        /// Род неопределен
        /// </summary>
        NotDefind = -1,

        /// <summary>
        /// Мужской род
        /// </summary>
        MasculineGender = 1,

        /// <summary>
        /// Женский род
        /// </summary>
        FeminineGender = 0
    }
}