﻿
using Extension.Tests.Utilities;
using FluentAssertions;
using Microsoft.DotNet.Interactive;
using Microsoft.DotNet.Interactive.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Extension.Tests
{
    public class LessonTests
    {
        private Challenge GetChallenge(string name = null)
        {
            return new Challenge(new EditableCode[] { }, name: name);
        }

        private List<EditableCode> GetEditableCode(string code)
        {
            return new List<EditableCode>
            {
                new EditableCode("csharp", code)
            };
        }

        [Fact]
        public async Task starting_to_an_unrevealed_challenge_directly_reveals_it()
        {
            var lesson = new Lesson();
            var challenge = GetChallenge();

            await lesson.StartChallengeAsync(challenge);

            challenge.Revealed.Should().BeTrue();
        }

        [Fact]
        public async Task starting_a_challenge_sets_the_current_challenge_to_it()
        {
            var lesson = new Lesson();
            var challenge = GetChallenge();

            await lesson.StartChallengeAsync(challenge);

            lesson.CurrentChallenge.Should().Be(challenge);
        }

        [Fact]
        public async Task teacher_can_start_a_challenge_using_challenge_name()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge1 = new Challenge(GetEditableCode("1"), name: "1");
            challenge1.OnCodeSubmittedAsync(async context =>
            {
                await context.StartChallengeAsync("3");
            });
            var challenge2 = new Challenge(GetEditableCode("2"), name: "2");
            var challenge3 = new Challenge(GetEditableCode("3"), name: "3");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            lesson.AddChallenge(challenge3);
            await lesson.StartChallengeAsync(challenge1);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge3);
        }

        [Fact]
        public async Task teacher_can_explicitly_start_the_next_challenge()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge1 = GetChallenge("1");
            challenge1.OnCodeSubmittedAsync(async context =>
            {
                await context.StartNextChallengeAsync();
            });
            var challenge2 = GetChallenge("2");
            var challenge3 = GetChallenge("3");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            lesson.AddChallenge(challenge3);
            await lesson.StartChallengeAsync(challenge1);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge2);
        }

        [Fact]
        public async Task explicitly_starting_the_next_challenge_at_last_challenge_does_nothing()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge1 = GetChallenge("1");
            challenge1.OnCodeSubmittedAsync(async context =>
            {
                await context.StartNextChallengeAsync();
            });
            lesson.AddChallenge(challenge1);
            await lesson.StartChallengeAsync(challenge1);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge1);
        }

        [Fact]
        public async Task if_all_rules_passed_and_teacher_does_not_start_a_challenge_then_start_next_challenge()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge1 = GetChallenge("1");
            challenge1.AddRule(c => c.Pass());
            challenge1.AddRule(c => c.Pass());
            var challenge2 = GetChallenge("2");
            var challenge3 = GetChallenge("3");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            lesson.AddChallenge(challenge3);
            await lesson.StartChallengeAsync(challenge1);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge2);
        }

        [Fact]
        public async Task if_not_all_rules_passed_and_teacher_does_not_start_a_challenge_then_do_not_start_next_challenge()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge1 = GetChallenge("1");
            challenge1.AddRule(c => c.Fail());
            challenge1.AddRule(c => c.Pass());
            var challenge2 = GetChallenge("2");
            var challenge3 = GetChallenge("3");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            lesson.AddChallenge(challenge3);
            await lesson.StartChallengeAsync(challenge1);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge1);
        }

        [Fact]
        public async Task if_no_rules_were_defined_and_teacher_does_not_start_a_challenge_then_start_next_challenge()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge1 = GetChallenge("1");
            var challenge2 = GetChallenge("2");
            var challenge3 = GetChallenge("3");
            lesson.AddChallenge(challenge1);
            lesson.AddChallenge(challenge2);
            lesson.AddChallenge(challenge3);
            await lesson.StartChallengeAsync(challenge1);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge2);
        }

        [Fact]
        public async Task if_current_challenge_is_last_challenge_and_teacher_does_not_start_a_challenge_then_stay_in_same_challenge()
        {
            var lesson = new Lesson();
            using var kernel = new CompositeKernel
            {
                new CSharpKernel()
            }.UseLessonEvaluateMiddleware(lesson);
            var challenge1 = GetChallenge("1");
            challenge1.AddRule(c => c.Pass());
            challenge1.AddRule(c => c.Pass());
            lesson.AddChallenge(challenge1);
            await lesson.StartChallengeAsync(challenge1);

            await kernel.SubmitCodeAsync("1+1");

            lesson.CurrentChallenge.Should().Be(challenge1);
        }
    }
}
