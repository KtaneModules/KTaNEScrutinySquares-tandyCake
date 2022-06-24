using System;
using System.Collections.Generic;
using System.Linq;
using Rnd = UnityEngine.Random;

public class SquareInfo {

    class ColorWrapper
    {
        public SquareColor c;
        public ColorWrapper(SquareColor c) { this.c = c; }
    }

    public static readonly Dictionary<Dir, Property> dirToProp = new Dictionary<Dir, Property>()
    {
        { Dir.Up, Property.Word },
        { Dir.Down, Property.WordColor },
        { Dir.Right, Property.Background },
        { Dir.Left, Property.Border }
    };

    private ColorWrapper _word, _wordColor, _backgroundColor, _borderColor;
    public Dir direction;
    public List<Property> alteredProps = new List<Property>(1);
    public SquareInfo(SquareColor word, SquareColor wordColor, SquareColor backgroundColor, SquareColor borderColor)
    {
        _word = new ColorWrapper(word);
        _wordColor = new ColorWrapper(wordColor);
        _backgroundColor = new ColorWrapper(backgroundColor);
        _borderColor = new ColorWrapper(borderColor);
    }   

    public SquareColor GetProp(Property p)
    {
        return GetPropWrapper(p).c;
    }
    private ColorWrapper GetPropWrapper(Property p)
    {
        switch (p)
        {
            case Property.Word: return _word;
            case Property.WordColor: return _wordColor;
            case Property.Background: return _backgroundColor;
            case Property.Border: return _borderColor;
        }
        throw new ArgumentOutOfRangeException("p");
    }
    public SquareColor GetProp(Dir d)
    {
        return GetPropWrapper(d).c;
    }
    private ColorWrapper GetPropWrapper(Dir d)
    {
        return GetPropWrapper(dirToProp[d]);
    }

    public void Alter(Dir d)
    {
        ColorWrapper prop = GetPropWrapper(d);
        SquareColor init = prop.c;

        Predicate<SquareColor> extraCondition = x => false;
        if (prop == _wordColor)
            extraCondition = x => x == _backgroundColor.c;
        if (prop == _backgroundColor)
            extraCondition = x => x == _wordColor.c;

        while (prop.c == init || extraCondition(prop.c))
            prop.c = (SquareColor)Rnd.Range(0, 8);
 
        alteredProps.Add(dirToProp[d]);
        UnityEngine.Debug.LogFormat("Changed {0} to {1}.", init, prop.c);
    }
    public void Alter()
    {
        Alter(direction);
    }
    public void AlterTwo()
    {
        int a = Rnd.Range(0, 4);
        int b;
        do b = Rnd.Range(0, 4);
        while (a == b);
        Alter((Dir)a);
        Alter((Dir)b);
    }
    
    public override string ToString()
    {
        return string.Format("the word {0} in {1} with {2} around it on a {3} background.", _word.c, _wordColor.c, _borderColor.c, _backgroundColor.c);
    }
}
