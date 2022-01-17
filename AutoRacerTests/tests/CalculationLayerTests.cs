using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace AutoRacerTests.Tests
{
  [TestFixture]
  public class CalculationLayerTests
  {
    private CalculationLayer _calcLayer = new CalculationLayer();

    [SetUp]
    public void SetUp()
    {
    }

    [Test]
    public void GetLevelValueFromCard_Name_Success()
    {
      var level = 1;
      var card = GetLevelTestCard(level);

      Assert.That(card.GetName(), Is.EqualTo($"name_success_C{level}"));
    }

    [Test]
    public void GetLevelValueFromCard_Description_Success()
    {
      var level = 1;
      var card = GetLevelTestCard(level);

      Assert.That(card.GetDescription(), Is.EqualTo($"description_success_C{level}"));
    }

    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    public void ApplyLevelValues_All_Success(int level)
    {
      var card = GetLevelTestCard(level);
      var leveledCard = _calcLayer.ApplyLevelValues(card);

      Assert.That(leveledCard.GetRawName(), Is.EqualTo($"name_success_C{level}"));
      Assert.That(leveledCard.GetRawDescription(), Is.EqualTo($"description_success_C{level}"));
      var moveTokenAbility = leveledCard.Abilities.MoveTokenAbilities.FirstOrDefault();
      Assert.That(moveTokenAbility.Name, Is.EqualTo($"name_success_A{level}"));
      Assert.That(moveTokenAbility.Phase, Is.EqualTo($"phase_success_A{level}"));
      Assert.That(moveTokenAbility.Duration, Is.EqualTo($"duration_success_A{level}"));
      Assert.That(moveTokenAbility.Value, Is.EqualTo($"value_success_A{level}"));
      Assert.That(moveTokenAbility.Type, Is.EqualTo($"type_success_A{level}"));
      var function = moveTokenAbility.Functions.FirstOrDefault();
      Assert.That(function.Key, Is.EqualTo($"key_success_F{level}"));
      for (int i = 0; i < function.BodyMultiline.ToArray().Length; i++)
      {
        var bodyLine = function.BodyMultiline.ToArray()[i];
        Assert.That(bodyLine, Is.Not.EqualTo($"line{i}_success_F{level}"));
      }
      var target = moveTokenAbility.Target;
      Assert.AreEqual(target.GetTargetType(), TargetType.All); // TODO
      Assert.AreEqual(target.GetDirection(), Direction.Any); // TODO
      Assert.AreEqual(target.GetPriority(), Priority.Closest); // TODO
      Assert.That(target.Amount, Is.EqualTo($"amount_success_T{level}"));
      Assert.That(target.Range.Min, Is.EqualTo($"min_success_R{level}"));
      Assert.That(target.Range.Max, Is.EqualTo($"max_success_R{level}"));
    }

    private Card GetLevelTestCard(int level)
    {
      return new Card
      {
        Name = "name_{testCard}",
        Description = "description_{testCard}",
        BaseMove = 1,
        Tier = 1,
        Level = level,
        Exp = 0,
        Abilities = new Abilities
        {
          MoveTokenAbilities = new List<MoveTokenAbility>() {
            new MoveTokenAbility {
              Name = "name_{testAbility}",
              Phase = "phase_{testAbility}",
              Functions = new List<Function>() {
                new Function {
                  Key = "key_{testFunction}",
                  BodyMultiline = new List<string>() {
                    "line0_{testFunction}",
                    "line1_{testFunction}",
                    "line2_{testFunction}"
                  }
                }
              },
              Duration = "duration_{testAbility}",
              Target = new Target {
                Type = "{testTargetType}", // TODO!!!!
                Direction = "{testTargetDirection}",
                Priority = "{testTargetPriority}",
                Amount = "amount_{testTarget}",
                Range = new Range {
                  Min = "min_{testRange}",
                  Max = "max_{testRange}"
                }
              },
              Value = "value_{testAbility}",
              Type = "type_{testAbility}"
            }
          },
          PrepAbilities = new List<PrepAbility>()
          {
            // TODO
          }
        },
        LevelValues = new List<LevelValue>() {
          new LevelValue {
            Id = 1,
            OutKeys = new List<OutKey>() {
              new OutKey {
                Key = "testCard",
                Value = "success_C1"
              },
              new OutKey {
                Key = "testAbility",
                Value = "success_A1"
              },
              new OutKey {
                Key = "testFunction",
                Value = "success_F1"
              },
              new OutKey {
                Key = "testTargetType",
                Value = "all"
              },
              new OutKey {
                Key = "testTargetDirection",
                Value = "any"
              },
              new OutKey {
                Key = "testTargetPriority",
                Value = "closest"
              },
              new OutKey {
                Key = "testTarget",
                Value = "success_T1"
              },
              new OutKey {
                Key = "testRange",
                Value = "success_R1"
              }
            }
          },
          new LevelValue {
            Id = 2,
            OutKeys = new List<OutKey>() {
              new OutKey {
                Key = "testCard",
                Value = "success_C2"
              },
              new OutKey {
                Key = "testAbility",
                Value = "success_A2"
              },
              new OutKey {
                Key = "testFunction",
                Value = "success_F2"
              },
              new OutKey {
                Key = "testTargetType",
                Value = "all"
              },
              new OutKey {
                Key = "testTargetDirection",
                Value = "any"
              },
              new OutKey {
                Key = "testTargetPriority",
                Value = "closest"
              },
              new OutKey {
                Key = "testTarget",
                Value = "success_T2"
              },
              new OutKey {
                Key = "testRange",
                Value = "success_R2"
              }
            }
          },
          new LevelValue {
            Id = 3,
            OutKeys = new List<OutKey>() {
              new OutKey {
                Key = "testCard",
                Value = "success_C3"
              },
              new OutKey {
                Key = "testAbility",
                Value = "success_A3"
              },
              new OutKey {
                Key = "testFunction",
                Value = "success_F3"
              },
              new OutKey {
                Key = "testTargetType",
                Value = "all"
              },
              new OutKey {
                Key = "testTargetDirection",
                Value = "any"
              },
              new OutKey {
                Key = "testTargetPriority",
                Value = "closest"
              },
              new OutKey {
                Key = "testTarget",
                Value = "success_T3"
              },
              new OutKey {
                Key = "testRange",
                Value = "success_R3"
              }
            }
          }
        }
      };
    }
  }
}
