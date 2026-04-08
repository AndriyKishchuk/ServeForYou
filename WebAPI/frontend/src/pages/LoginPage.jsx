import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { extractErrorMessage } from "../api/api";
import { useAuth } from "../context/AuthContext";

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [form, setForm] = useState({ email: "", password: "" });
  const [error, setError] = useState("");
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleChange = ({ target }) => {
    setForm((current) => ({ ...current, [target.name]: target.value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      const data = await login(form);
      navigate(data.redirectUrl, { replace: true });
    } catch (err) {
      setError(extractErrorMessage(err, "Invalid email or password."));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="auth-layout">
      <section className="auth-showcase">
        <span className="hero-eyebrow">ServeForYou</span>
        <h1>One service for managers, employees and finished work with files attached.</h1>
        <p>
          Clean assignments, a calmer workspace, secure file exchange and direct routes for customers,
          managers and employees.
        </p>

        <div className="auth-feature-list">
          <article>
            <strong>Manager control</strong>
            <span>Create tasks, assign employees, attach briefings and track delivery.</span>
          </article>
          <article>
            <strong>Employee focus</strong>
            <span>See only your work, update statuses fast and upload execution results.</span>
          </article>
          <article>
            <strong>Modern flow</strong>
            <span>Beautiful responsive UI, protected routes and zero confusion after sign in.</span>
          </article>
        </div>
      </section>

      <section className="auth-card">
        <div className="auth-card-head">
          <span className="pill-label">Welcome back</span>
          <h2>Sign in to your workspace</h2>
          <p>Use your email and continue inside the ServeForYou workspace.</p>
        </div>

        <form className="auth-form" onSubmit={handleSubmit}>
          <label>
            Email
            <input
              name="email"
              type="email"
              placeholder="name@company.com"
              value={form.email}
              onChange={handleChange}
              required
            />
          </label>

          <label>
            Password
            <input
              name="password"
              type="password"
              placeholder="Your secure password"
              value={form.password}
              onChange={handleChange}
              required
            />
          </label>

          {error ? <div className="form-error">{error}</div> : null}

          <button className="primary-button wide" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Signing in..." : "Enter dashboard"}
          </button>
        </form>

        <p className="auth-footer">
          No account yet? <Link to="/register">Create one now</Link>
        </p>
      </section>
    </div>
  );
}
