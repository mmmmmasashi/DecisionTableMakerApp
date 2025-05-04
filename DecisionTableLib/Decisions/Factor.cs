using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DecisionTableLib.Decisions
{
    public record Factor
    {
        public string Name { get; }
        public List<Level> Levels { get; }
        public Factor(string name, List<Level> levels)
        {
            Name = name;
            Levels = levels;
        }

        public virtual bool Equals(Factor? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            // Name の等価性と Levels の内容の等価性を比較
            return Name == other.Name &&
                   Levels.SequenceEqual(other.Levels);
        }

        public override int GetHashCode()
        {
            // Levels の内容を考慮したハッシュコードを生成
            return HashCode.Combine(Name, Levels.Aggregate(0, (hash, level) => hash ^ level.GetHashCode()));
        }
    }
}
