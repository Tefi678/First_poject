#include <iostream>
#include <thread>
#include <mutex>
#include <condition_variable>

class LectoresEscritores {
public:
    LectoresEscritores() : lectores(0), escritor_activo(false) {}

    void leer() {
        std::unique_lock<std::mutex> lock(mtx);
        // Esperar hasta que no haya escritores activos
        cv_lectores.wait(lock, [this]() { return !escritor_activo; });
        ++lectores;
        lock.unlock();

        // Realizar la lectura
        std::cout << "Leyendo... (Lectores activos: " << lectores << ")\n";

        lock.lock();
        --lectores;
        if (lectores == 0) {
            cv_escritores.notify_one(); // Notificar a escritores si ninguno está leyendo
        }
        lock.unlock();
    }

    void escribir() {
        std::unique_lock<std::mutex> lock(mtx);
        // Esperar hasta que no haya lectores ni escritores activos
        cv_escritores.wait(lock, [this]() { return !escritores_activos() && !escritor_activo; });
        escritor_activo = true;
        lock.unlock();

        // Realizar la escritura
        std::cout << "Escribiendo...\n";

        lock.lock();
        escritor_activo = false;
        cv_lectores.notify_all(); // Notificar a los lectores
        cv_escritores.notify_one(); // Notificar a un escritor en espera
        lock.unlock();
    }

private:
    std::mutex mtx;
    std::condition_variable cv_lectores;
    std::condition_variable cv_escritores;
    int lectores;
    bool escritor_activo;

    bool escritores_activos() {
        return escritor_activo;
    }
};

void funcion_lector(LectoresEscritores& recurso) {
    recurso.leer();
}

void funcion_escritor(LectoresEscritores& recurso) {
    recurso.escribir();
}

int main() {
    LectoresEscritores recurso;

    // Crear hilos de lectores
    std::thread lector1(funcion_lector, std::ref(recurso));
    std::thread lector2(funcion_lector, std::ref(recurso));
    std::thread lector3(funcion_lector, std::ref(recurso));

    // Crear hilos de escritores
    std::thread escritor1(funcion_escritor, std::ref(recurso));
    std::thread escritor2(funcion_escritor, std::ref(recurso));

    // Unir los hilos
    lector1.join();
    lector2.join();
    lector3.join();
    escritor1.join();
    escritor2.join();

    return 0;
}
