import React from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../context/AuthContext";

export default function Header({ profile }) {
    const { logout } = useAuth();
    const navigate = useNavigate();

    return (
        <header className="flex items-center justify-between bg-white px-6 py-4 shadow">
            <div className="flex items-center space-x-4">
                <Link to="/" className="text-[color:var(--color-brand)] text-xl font-bold">
                    PsyTest
                </Link>
            </div>
            <div className="flex items-center space-x-4">
                {profile && (
                    <span className="text-gray-700">
                        {profile.firstName} {profile.lastName}
                    </span>
                )}
                <Link
                    to="/profile"
                    className="bg-[color:var(--color-brand)] text-white px-3 py-1 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                >
                    Личный кабинет
                </Link>
                <button
                    onClick={() => {
                        logout();
                        navigate("/login");
                    }}
                    className="bg-[color:var(--color-brand)] text-white px-3 py-1 rounded-lg hover:bg-[color:var(--color-brand-dark)] transition"
                >
                    Выйти
                </button>
            </div>
        </header>
    );
}
