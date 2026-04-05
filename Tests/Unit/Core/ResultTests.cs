namespace Tests.Unit.Core;

public class ResultGenericTests
{
    #region Construction

    [Fact]
    public void Success_HasValue_IsSuccess()
    {
        Result<int> result = Result<int>.Success(42);
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Value);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void Failure_HasErrorMessage_IsNotSuccess()
    {
        Result<int> result = Result<int>.Failure("something went wrong");
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
        Assert.Equal("something went wrong", result.ErrorMessage);
    }

    #endregion

    #region Match

    [Fact]
    public void Match_OnSuccess_CallsSuccessHandler()
    {
        Result<int> result = Result<int>.Success(42);
        string output = result.Match(v => $"got {v}", e => $"error: {e}");
        Assert.Equal("got 42", output);
    }

    [Fact]
    public void Match_OnFailure_CallsFailureHandler()
    {
        Result<int> result = Result<int>.Failure("oops");
        string output = result.Match(v => $"got {v}", e => $"error: {e}");
        Assert.Equal("error: oops", output);
    }

    #endregion

    #region Map

    [Fact]
    public void Map_OnSuccess_TransformsValue()
    {
        Result<int> result = Result<int>.Success(5);
        Result<string> mapped = result.Map(v => v.ToString());
        Assert.True(mapped.IsSuccess);
        Assert.Equal("5", mapped.Value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        Result<int> result = Result<int>.Failure("oops");
        Result<string> mapped = result.Map(v => v.ToString());
        Assert.True(mapped.IsFailure);
        Assert.Equal("oops", mapped.ErrorMessage);
    }

    [Fact]
    public void Map_ChainedMaps_ComposeCorrectly()
    {
        Result<int> result = Result<int>.Success(3);
        Result<string> mapped = result.Map(v => v * 2).Map(v => $"value={v}");
        Assert.Equal("value=6", mapped.Value);
    }

    #endregion

    #region Bind

    [Fact]
    public void Bind_OnSuccess_CallsBinder()
    {
        Result<int> result = Result<int>.Success(5);
        Result<string> bound = result.Bind(v => Result<string>.Success($"num:{v}"));
        Assert.True(bound.IsSuccess);
        Assert.Equal("num:5", bound.Value);
    }

    [Fact]
    public void Bind_OnFailure_PropagatesError()
    {
        Result<int> result = Result<int>.Failure("fail");
        Result<string> bound = result.Bind(v => Result<string>.Success($"num:{v}"));
        Assert.True(bound.IsFailure);
        Assert.Equal("fail", bound.ErrorMessage);
    }

    [Fact]
    public void Bind_BinderReturnsFailure_PropagatesNewError()
    {
        Result<int> result = Result<int>.Success(5);
        Result<string> bound = result.Bind<string>(v => Result<string>.Failure("bind failed"));
        Assert.True(bound.IsFailure);
        Assert.Equal("bind failed", bound.ErrorMessage);
    }

    #endregion

    #region Apply

    [Fact]
    public void Apply_BothSuccess_AppliesFunction()
    {
        Result<int> value = Result<int>.Success(5);
        Result<Func<int, string>> func = Result<Func<int, string>>.Success(v => $"val={v}");
        Result<string> applied = value.Apply(func);
        Assert.True(applied.IsSuccess);
        Assert.Equal("val=5", applied.Value);
    }

    [Fact]
    public void Apply_FuncFailure_PropagatesError()
    {
        Result<int> value = Result<int>.Success(5);
        Result<Func<int, string>> func = Result<Func<int, string>>.Failure("func error");
        Result<string> applied = value.Apply(func);
        Assert.True(applied.IsFailure);
        Assert.Equal("func error", applied.ErrorMessage);
    }

    [Fact]
    public void Apply_ValueFailure_PropagatesError()
    {
        Result<int> value = Result<int>.Failure("value error");
        Result<Func<int, string>> func = Result<Func<int, string>>.Success(v => $"val={v}");
        Result<string> applied = value.Apply(func);
        Assert.True(applied.IsFailure);
    }

    #endregion

    #region Side Effects

    [Fact]
    public void OnSuccess_CallsAction_WhenSuccess()
    {
        int captured = 0;
        Result<int>.Success(42).OnSuccess(v => captured = v);
        Assert.Equal(42, captured);
    }

    [Fact]
    public void OnSuccess_DoesNotCallAction_WhenFailure()
    {
        int captured = 0;
        Result<int>.Failure("oops").OnSuccess(v => captured = v);
        Assert.Equal(0, captured);
    }

    [Fact]
    public void OnFailure_CallsAction_WhenFailure()
    {
        string captured = "";
        Result<int>.Failure("oops").OnFailure(e => captured = e);
        Assert.Equal("oops", captured);
    }

    [Fact]
    public void Tap_ReturnsSameResult()
    {
        Result<int> original = Result<int>.Success(42);
        Result<int> tapped = original.Tap(_ => { });
        Assert.Same(original, tapped);
    }

    #endregion

    #region GetValue

    [Fact]
    public void GetValueOrDefault_ReturnsValue_WhenSuccess()
    {
        Assert.Equal(42, Result<int>.Success(42).GetValueOrDefault(0));
    }

    [Fact]
    public void GetValueOrDefault_ReturnsDefault_WhenFailure()
    {
        Assert.Equal(0, Result<int>.Failure("oops").GetValueOrDefault(0));
    }

    [Fact]
    public void GetValueOrDefault_Factory_ReturnsValue_WhenSuccess()
    {
        Assert.Equal(42, Result<int>.Success(42).GetValueOrDefault(() => 99));
    }

    [Fact]
    public void GetValueOrDefault_Factory_ReturnsDefault_WhenFailure()
    {
        Assert.Equal(99, Result<int>.Failure("oops").GetValueOrDefault(() => 99));
    }

    [Fact]
    public void GetValueOrThrow_ReturnsValue_WhenSuccess()
    {
        Assert.Equal(42, Result<int>.Success(42).GetValueOrThrow());
    }

    [Fact]
    public void GetValueOrThrow_Throws_WhenFailure()
    {
        Assert.Throws<InvalidOperationException>(() => Result<int>.Failure("oops").GetValueOrThrow());
    }

    #endregion

    #region Implicit Conversions

    [Fact]
    public void ImplicitConversion_FromValue_CreatesSuccess()
    {
        Result<int> result = 42;
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void ImplicitConversion_ToBool_ReflectsIsSuccess()
    {
        Assert.True(Result<int>.Success(1));
        Assert.False(Result<int>.Failure("oops"));
    }

    #endregion

    #region Equality

    [Fact]
    public void Equals_TwoSuccessWithSameValue_AreEqual()
    {
        Assert.Equal(Result<int>.Success(42), Result<int>.Success(42));
    }

    [Fact]
    public void Equals_TwoFailureWithSameMessage_AreEqual()
    {
        Assert.Equal(Result<int>.Failure("oops"), Result<int>.Failure("oops"));
    }

    [Fact]
    public void Equals_SuccessAndFailure_AreNotEqual()
    {
        Assert.NotEqual(Result<int>.Success(0), Result<int>.Failure("oops"));
    }

    [Fact]
    public void GetHashCode_EqualResults_HaveSameHashCode()
    {
        int h1 = Result<int>.Success(42).GetHashCode();
        int h2 = Result<int>.Success(42).GetHashCode();
        Assert.Equal(h1, h2);
    }

    #endregion

    #region ToString

    [Fact]
    public void ToString_Success_ContainsValue()
    {
        Assert.Equal("Success(42)", Result<int>.Success(42).ToString());
    }

    [Fact]
    public void ToString_Failure_ContainsError()
    {
        Assert.Equal("Failure(oops)", Result<int>.Failure("oops").ToString());
    }

    #endregion
}

public class ResultNonGenericTests
{
    #region Success and Failure

    [Fact]
    public void Success_IsSuccess()
    {
        Result result = Result.Success();
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
    }

    [Fact]
    public void Failure_IsFailure()
    {
        Result result = Result.Failure("oops");
        Assert.True(result.IsFailure);
        Assert.Equal("oops", result.ErrorMessage);
    }

    #endregion

    #region Match

    [Fact]
    public void Match_OnSuccess_CallsSuccessFunc()
    {
        string output = Result.Success().Match(() => "ok", e => $"err: {e}");
        Assert.Equal("ok", output);
    }

    [Fact]
    public void Match_OnFailure_CallsFailureFunc()
    {
        string output = Result.Failure("oops").Match(() => "ok", e => $"err: {e}");
        Assert.Equal("err: oops", output);
    }

    [Fact]
    public void Match_Action_OnSuccess_CallsSuccessAction()
    {
        bool called = false;
        Result.Success().Match(() => called = true, _ => { });
        Assert.True(called);
    }

    #endregion

    #region Bind

    [Fact]
    public void Bind_OnSuccess_CallsBinder()
    {
        Result result = Result.Success().Bind(() => Result.Failure("inner fail"));
        Assert.True(result.IsFailure);
        Assert.Equal("inner fail", result.ErrorMessage);
    }

    [Fact]
    public void Bind_OnFailure_PropagatesError()
    {
        bool binderCalled = false;
        Result result = Result.Failure("outer").Bind(() => { binderCalled = true; return Result.Success(); });
        Assert.False(binderCalled);
        Assert.Equal("outer", result.ErrorMessage);
    }

    [Fact]
    public void BindGeneric_OnSuccess_CallsBinder()
    {
        Result<int> result = Result.Success().Bind(() => Result<int>.Success(42));
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    #endregion

    #region Map

    [Fact]
    public void Map_OnSuccess_CreatesGenericResult()
    {
        Result<int> result = Result.Success().Map(() => 42);
        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void Map_OnFailure_PropagatesError()
    {
        Result<int> result = Result.Failure("oops").Map(() => 42);
        Assert.True(result.IsFailure);
    }

    #endregion

    #region Side Effects

    [Fact]
    public void OnSuccess_CallsAction_WhenSuccess()
    {
        bool called = false;
        Result.Success().OnSuccess(() => called = true);
        Assert.True(called);
    }

    [Fact]
    public void OnFailure_CallsAction_WhenFailure()
    {
        string captured = "";
        Result.Failure("oops").OnFailure(e => captured = e);
        Assert.Equal("oops", captured);
    }

    [Fact]
    public void Tap_ReturnsSameResult()
    {
        Result original = Result.Success();
        Result tapped = original.Tap(() => { });
        Assert.Same(original, tapped);
    }

    #endregion

    #region ThrowOnFailure

    [Fact]
    public void ThrowOnFailure_Success_DoesNotThrow()
    {
        Result.Success().ThrowOnFailure();
    }

    [Fact]
    public void ThrowOnFailure_Failure_ThrowsInvalidOperation()
    {
        Assert.Throws<InvalidOperationException>(() => Result.Failure("oops").ThrowOnFailure());
    }

    #endregion

    #region Combine

    [Fact]
    public void Combine_AllSuccess_ReturnsSuccess()
    {
        Result combined = Result.Combine(Result.Success(), Result.Success(), Result.Success());
        Assert.True(combined.IsSuccess);
    }

    [Fact]
    public void Combine_OneFailure_ReturnsFirstFailure()
    {
        Result combined = Result.Combine(Result.Success(), Result.Failure("first"), Result.Failure("second"));
        Assert.True(combined.IsFailure);
        Assert.Equal("first", combined.ErrorMessage);
    }

    [Fact]
    public void Combine_Enumerable_AllSuccess()
    {
        Result combined = Result.Combine(new[] { Result.Success(), Result.Success() }.AsEnumerable());
        Assert.True(combined.IsSuccess);
    }

    #endregion

    #region Implicit and Equality

    [Fact]
    public void ImplicitBool_ReflectsIsSuccess()
    {
        Assert.True(Result.Success());
        Assert.False(Result.Failure("oops"));
    }

    [Fact]
    public void Equals_TwoSuccess_AreEqual()
    {
        Assert.Equal(Result.Success(), Result.Success());
    }

    [Fact]
    public void Equals_TwoFailureSameMessage_AreEqual()
    {
        Assert.Equal(Result.Failure("oops"), Result.Failure("oops"));
    }

    [Fact]
    public void Equals_SuccessAndFailure_NotEqual()
    {
        Assert.NotEqual(Result.Success(), Result.Failure("oops"));
    }

    #endregion
}
