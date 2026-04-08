import { Navigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function ProtectedRoute({ allowedRoles, children }) {
  const { auth, isAuthenticated, isBootstrapping, redirectPath } = useAuth();

  if (isBootstrapping) {
    return <div className="screen-loader">Checking access...</div>;
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (allowedRoles?.length && !allowedRoles.includes(auth?.role)) {
    return <Navigate to={redirectPath} replace />;
  }

  return children;
}
