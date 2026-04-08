import { createContext, useContext, useEffect, useState } from "react";
import { authApi, clearStoredAuth, getStoredAuth, setStoredAuth, userApi } from "../api/api";

const AuthContext = createContext(null);

function resolveRedirectPath(auth) {
  return auth?.redirectUrl || "/login";
}

export function AuthProvider({ children }) {
  const [auth, setAuth] = useState(getStoredAuth());
  const [profile, setProfile] = useState(null);
  const [isBootstrapping, setIsBootstrapping] = useState(true);

  useEffect(() => {
    let isMounted = true;

    async function bootstrap() {
      const storedAuth = getStoredAuth();

      if (!storedAuth?.token) {
        if (isMounted) {
          setIsBootstrapping(false);
        }
        return;
      }

      try {
        const [{ data: authData }, { data: profileData }] = await Promise.all([
          authApi.me(),
          userApi.getMe(),
        ]);

        const mergedAuth = { ...storedAuth, ...authData, token: storedAuth.token };

        if (isMounted) {
          setAuth(mergedAuth);
          setProfile(profileData);
          setStoredAuth(mergedAuth);
        }
      } catch {
        clearStoredAuth();
        if (isMounted) {
          setAuth(null);
          setProfile(null);
        }
      } finally {
        if (isMounted) {
          setIsBootstrapping(false);
        }
      }
    }

    bootstrap();

    return () => {
      isMounted = false;
    };
  }, []);

  const login = async (credentials) => {
    const { data } = await authApi.login(credentials);
    setStoredAuth(data);
    setAuth(data);

    try {
      const { data: profileData } = await userApi.getMe();
      setProfile(profileData);
    } catch {
      setProfile(null);
    }

    return data;
  };

  const register = async (payload) => {
    const { data } = await authApi.register(payload);
    setStoredAuth(data);
    setAuth(data);

    try {
      const { data: profileData } = await userApi.getMe();
      setProfile(profileData);
    } catch {
      setProfile(null);
    }

    return data;
  };

  const logout = () => {
    clearStoredAuth();
    setAuth(null);
    setProfile(null);
  };

  const refreshProfile = async () => {
    const [{ data: authData }, { data: profileData }] = await Promise.all([
      authApi.me(),
      userApi.getMe(),
    ]);

    const storedAuth = getStoredAuth();
    const nextAuth = {
      ...(storedAuth || auth || {}),
      ...authData,
      token: storedAuth?.token || auth?.token || "",
    };

    setAuth(nextAuth);
    setProfile(profileData);
    setStoredAuth(nextAuth);

    return { auth: nextAuth, profile: profileData };
  };

  return (
    <AuthContext.Provider
      value={{
        auth,
        profile,
        isBootstrapping,
        isAuthenticated: Boolean(auth?.token),
        redirectPath: resolveRedirectPath(auth),
        login,
        register,
        logout,
        refreshProfile,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  return useContext(AuthContext);
}
