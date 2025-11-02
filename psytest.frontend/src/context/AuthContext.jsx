import React, { createContext, useContext, useState, useEffect } from "react";

const AuthContext = createContext();

export function AuthProvider({ children }) {
    const [token, setToken] = useState(localStorage.getItem("token"));
    const [profile, setProfile] = useState(null);

    // Когда у нас есть токен — тянем профиль
    useEffect(() => {
        if (!token) {
            setProfile(null);
            return;
        }

        // грузим профиль текущего пользователя
        fetch(`${import.meta.env.VITE_IDENTITY_URL}/profile`, {
            method: "GET",
            headers: {
                Authorization: `Bearer ${token}`,
            },
        })
            .then((res) => {
                if (!res.ok) {
                    // токен невалиден/протух и т.д.
                    throw new Error("Не удалось получить профиль");
                }
                return res.json();
            })
            .then((data) => {
                setProfile(data);
            })
            .catch(() => {
                // если что-то пошло не так, считаем что не залогинен
                setProfile(null);
            });
    }, [token]);

    // вызывается после успешного логина
    const login = (newToken) => {
        localStorage.setItem("token", newToken);
        setToken(newToken);
    };

    const logout = () => {
        localStorage.removeItem("token");
        setToken(null);
        setProfile(null);
    };

    return (
        <AuthContext.Provider
            value={{
                token,
                profile,
                login,
                logout,
            }}
        >
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    return useContext(AuthContext);
}
