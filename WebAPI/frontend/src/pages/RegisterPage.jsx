import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { extractErrorMessage } from "../api/api";
import { useAuth } from "../context/AuthContext";

export default function RegisterPage() {
  const navigate = useNavigate();
  const { register } = useAuth();
  const [form, setForm] = useState({
    name: "",
    surname: "",
    email: "",
    password: "",
    role: "Employee",
  });
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState("");

  const handleChange = ({ target }) => {
    setForm((current) => ({ ...current, [target.name]: target.value }));
  };

  const handleSubmit = async (event) => {
    event.preventDefault();
    setError("");
    setIsSubmitting(true);

    try {
      const data = await register(form);
      navigate(data.redirectUrl, { replace: true });
    } catch (err) {
      setError(extractErrorMessage(err, "Registration failed."));
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="auth-layout">
      <section className="auth-showcase warm">
        <span className="hero-eyebrow">Launch your workspace</span>
        <h1>Register once and land in the right dashboard automatically.</h1>
        <p>
          Managers jump into assignment mode, employees land in their task queue, and everyone joins the
          same ServeForYou workspace without picking a company manually.
        </p>
      </section>

      <section className="auth-card">
        <div className="auth-card-head">
          <span className="pill-label">Create account</span>
          <h2>Join your company space</h2>
          <p>Pick a role, create your account and we will attach you to ServeForYou automatically.</p>
        </div>

        <form className="auth-form" onSubmit={handleSubmit}>
          <div className="form-grid two-columns">
            <label>
              First name
              <input name="name" value={form.name} onChange={handleChange} placeholder="Andrii" required />
            </label>
            <label>
              Last name
              <input name="surname" value={form.surname} onChange={handleChange} placeholder="Koval" required />
            </label>
          </div>

          <label>
            Email
            <input
              name="email"
              type="email"
              value={form.email}
              onChange={handleChange}
              placeholder="name@company.com"
              required
            />
          </label>

          <label>
            Password
            <input
              name="password"
              type="password"
              value={form.password}
              onChange={handleChange}
              placeholder="At least one strong password"
              required
            />
          </label>

          <div className="form-grid two-columns">
            <label>
              Role
              <select name="role" value={form.role} onChange={handleChange}>
                <option value="Admin">Admin</option>
                <option value="Manager">Manager</option>
                <option value="Employee">Employee</option>
              </select>
            </label>
            <label>
              Company
              <input value="ServeForYou" disabled />
            </label>
          </div>

          {error ? <div className="form-error">{error}</div> : null}

          <button className="primary-button wide" type="submit" disabled={isSubmitting}>
            {isSubmitting ? "Creating account..." : "Register and continue"}
          </button>
        </form>

        <p className="auth-footer">
          Already registered? <Link to="/login">Sign in</Link>
        </p>
      </section>
    </div>
  );
}
