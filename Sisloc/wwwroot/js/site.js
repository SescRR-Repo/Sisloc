// wwwroot/js/site.js - Substituir o conteúdo existente

/**
 * Sisloc - Sistema de Agendamento de Veículos
 * JavaScript principal
 */

// Configurações globais
const SislocConfig = {
    autoHideAlerts: 5000, // 5 segundos
    animationDuration: 300,
    dateFormat: 'dd/MM/yyyy',
    timeFormat: 'HH:mm'
};

// Utilitários globais
const SislocUtils = {

    // Formatar data para exibição
    formatDate: function (date) {
        if (!date) return '';
        const d = new Date(date);
        return d.toLocaleDateString('pt-BR');
    },

    // Formatar data e hora para exibição
    formatDateTime: function (date) {
        if (!date) return '';
        const d = new Date(date);
        return d.toLocaleDateString('pt-BR') + ' ' + d.toLocaleTimeString('pt-BR', { hour: '2-digit', minute: '2-digit' });
    },

    // Copiar texto para clipboard
    copyToClipboard: function (text) {
        if (navigator.clipboard) {
            navigator.clipboard.writeText(text).then(() => {
                SislocUtils.showToast('Texto copiado!', 'success');
            });
        } else {
            // Fallback para navegadores mais antigos
            const textarea = document.createElement('textarea');
            textarea.value = text;
            document.body.appendChild(textarea);
            textarea.select();
            document.execCommand('copy');
            document.body.removeChild(textarea);
            SislocUtils.showToast('Texto copiado!', 'success');
        }
    },

    // Mostrar toast notification
    showToast: function (message, type = 'info') {
        const toast = document.createElement('div');
        toast.className = `alert alert-${type} alert-dismissible fade show position-fixed`;
        toast.style.cssText = 'top: 20px; right: 20px; z-index: 9999; min-width: 300px;';
        toast.innerHTML = `
            ${message}
            <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
        `;

        document.body.appendChild(toast);

        // Auto remove
        setTimeout(() => {
            if (toast.parentNode) {
                toast.remove();
            }
        }, SislocConfig.autoHideAlerts);
    },

    // Confirmar ação
    confirm: function (message, callback) {
        if (confirm(message)) {
            if (typeof callback === 'function') {
                callback();
            }
            return true;
        }
        return false;
    },

    // Validar CPF (para futuras implementações)
    validateCPF: function (cpf) {
        cpf = cpf.replace(/[^\d]+/g, '');
        if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) return false;

        let soma = 0;
        for (let i = 0; i < 9; i++) {
            soma += parseInt(cpf.charAt(i)) * (10 - i);
        }
        let resto = 11 - (soma % 11);
        if (resto === 10 || resto === 11) resto = 0;
        if (resto !== parseInt(cpf.charAt(9))) return false;

        soma = 0;
        for (let i = 0; i < 10; i++) {
            soma += parseInt(cpf.charAt(i)) * (11 - i);
        }
        resto = 11 - (soma % 11);
        if (resto === 10 || resto === 11) resto = 0;
        return resto === parseInt(cpf.charAt(10));
    },

    // Máscara para telefone
    phoneMask: function (value) {
        return value
            .replace(/\D/g, '')
            .replace(/(\d{2})(\d)/, '($1) $2')
            .replace(/(\d{4})(\d)/, '$1-$2')
            .replace(/(\d{4})-(\d)(\d{4})/, '$1$2-$3')
            .replace(/(-\d{4})\d+?$/, '$1');
    },

    // Máscara para protocolo
    protocolMask: function (value) {
        return value.replace(/[^A-Z0-9]/g, '').substring(0, 20);
    }
};

// Inicialização quando o DOM estiver carregado
document.addEventListener('DOMContentLoaded', function () {

    // Auto-hide alerts
    setTimeout(function () {
        const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
        alerts.forEach(alert => {
            if (alert.classList.contains('show')) {
                const bsAlert = new bootstrap.Alert(alert);
                bsAlert.close();
            }
        });
    }, SislocConfig.autoHideAlerts);

    // Inicializar tooltips
    const tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"], [title]'));
    tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Configurar campos de data para data mínima
    const dateInputs = document.querySelectorAll('input[type="datetime-local"], input[type="date"]');
    const now = new Date();
    const minDate = now.toISOString().slice(0, 16);

    dateInputs.forEach(input => {
        if (!input.hasAttribute('min')) {
            input.setAttribute('min', minDate);
        }
    });

    // Máscara para campos de protocolo
    const protocolInputs = document.querySelectorAll('input[name="protocolo"], .protocol-input');
    protocolInputs.forEach(input => {
        input.addEventListener('input', function () {
            this.value = SislocUtils.protocolMask(this.value.toUpperCase());
        });
    });

    // Máscara para campos de telefone
    const phoneInputs = document.querySelectorAll('input[type="tel"], .phone-input');
    phoneInputs.forEach(input => {
        input.addEventListener('input', function () {
            this.value = SislocUtils.phoneMask(this.value);
        });
    });

    // Confirmar ações perigosas
    const dangerButtons = document.querySelectorAll('.btn-danger, [data-confirm]');
    dangerButtons.forEach(button => {
        button.addEventListener('click', function (e) {
            const message = this.getAttribute('data-confirm') || 'Tem certeza que deseja executar esta ação?';
            if (!confirm(message)) {
                e.preventDefault();
                return false;
            }
        });
    });

    // Auto-focus no primeiro campo obrigatório
    const firstRequired = document.querySelector('input[required], select[required], textarea[required]');
    if (firstRequired && !firstRequired.value) {
        firstRequired.focus();
    }

    // Animações de entrada
    const animatedElements = document.querySelectorAll('.card, .alert');
    animatedElements.forEach((element, index) => {
        element.style.opacity = '0';
        element.style.transform = 'translateY(20px)';

        setTimeout(() => {
            element.style.transition = `opacity ${SislocConfig.animationDuration}ms ease, transform ${SislocConfig.animationDuration}ms ease`;
            element.style.opacity = '1';
            element.style.transform = 'translateY(0)';
        }, index * 100);
    });
});

// Validações específicas do formulário de agendamento
if (document.querySelector('#agendamentoForm')) {
    document.addEventListener('DOMContentLoaded', function () {
        const form = document.querySelector('#agendamentoForm');
        const dataPartida = document.querySelector('#DataPartida');
        const dataChegada = document.querySelector('#DataChegada');

        // Validação de datas em tempo real
        function validarDatas() {
            if (!dataPartida.value || !dataChegada.value) return;

            const partida = new Date(dataPartida.value);
            const chegada = new Date(dataChegada.value);
            const agora = new Date();

            // Limpar mensagens anteriores
            dataPartida.setCustomValidity('');
            dataChegada.setCustomValidity('');

            // Validar data de partida
            if (partida < agora) {
                dataPartida.setCustomValidity('A data de partida não pode ser anterior ao momento atual.');
                return false;
            }

            // Validar data de chegada
            if (chegada <= partida) {
                dataChegada.setCustomValidity('A data de chegada deve ser posterior à data de partida.');
                return false;
            }

            return true;
        }

        dataPartida.addEventListener('change', validarDatas);
        dataChegada.addEventListener('change', validarDatas);

        // Valores padrão inteligentes
        if (!dataPartida.value) {
            const amanha = new Date();
            amanha.setDate(amanha.getDate() + 1);
            amanha.setHours(8, 0, 0, 0);
            dataPartida.value = amanha.toISOString().slice(0, 16);
        }

        if (!dataChegada.value) {
            const retorno = new Date(dataPartida.value || Date.now());
            retorno.setHours(17, 0, 0, 0);
            dataChegada.value = retorno.toISOString().slice(0, 16);
        }

        // Validação antes do envio
        form.addEventListener('submit', function (e) {
            if (!validarDatas()) {
                e.preventDefault();
                SislocUtils.showToast('Por favor, verifique as datas informadas.', 'danger');
                return false;
            }

            // Confirmação final
            const confirmMessage = 'Confirma o envio da solicitação de agendamento?';
            if (!confirm(confirmMessage)) {
                e.preventDefault();
                return false;
            }

            // Mostrar loading
            const submitBtn = form.querySelector('button[type="submit"]');
            const originalText = submitBtn.innerHTML;
            submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Enviando...';
            submitBtn.disabled = true;

            // Em caso de erro, restaurar botão
            setTimeout(() => {
                submitBtn.innerHTML = originalText;
                submitBtn.disabled = false;
            }, 10000);
        });
    });
}

// Funções específicas para consulta
const ConsultaFunctions = {

    // Buscar protocolo
    buscarProtocolo: function (protocolo) {
        if (!protocolo || protocolo.length < 10) {
            SislocUtils.showToast('Digite um protocolo válido.', 'warning');
            return false;
        }

        // Aqui poderia haver uma requisição AJAX para busca em tempo real
        return true;
    },

    // Copiar protocolo
    copiarProtocolo: function (button, protocolo) {
        SislocUtils.copyToClipboard(protocolo);

        // Feedback visual no botão
        const originalContent = button.innerHTML;
        button.innerHTML = '<i class="fas fa-check me-2"></i>Copiado!';
        button.classList.remove('btn-outline-primary');
        button.classList.add('btn-success');

        setTimeout(() => {
            button.innerHTML = originalContent;
            button.classList.remove('btn-success');
            button.classList.add('btn-outline-primary');
        }, 2000);
    }
};

// Funções específicas para administração
const AdminFunctions = {

    // Confirmar aprovação
    confirmarAprovacao: function (protocolo) {
        return SislocUtils.confirm(`Confirma a aprovação do agendamento ${protocolo}?`);
    },

    // Confirmar reprovação
    confirmarReprovacao: function (protocolo) {
        return SislocUtils.confirm(`Confirma a reprovação do agendamento ${protocolo}?`);
    },

    // Filtrar tabela
    filtrarTabela: function () {
        // Esta função poderia implementar filtros em tempo real
        console.log('Aplicando filtros...');
    },

    // Auto-refresh do dashboard
    enableAutoRefresh: function (interval = 60000) {
        setInterval(() => {
            const url = new URL(window.location);
            url.searchParams.set('autoRefresh', 'true');

            fetch(url)
                .then(response => response.text())
                .then(html => {
                    // Atualizar apenas o conteúdo da tabela
                    const parser = new DOMParser();
                    const doc = parser.parseFromString(html, 'text/html');
                    const newTable = doc.querySelector('.table-responsive');
                    const currentTable = document.querySelector('.table-responsive');

                    if (newTable && currentTable) {
                        currentTable.innerHTML = newTable.innerHTML;
                        SislocUtils.showToast('Dashboard atualizado.', 'info');
                    }
                })
                .catch(error => {
                    console.error('Erro ao atualizar dashboard:', error);
                });
        }, interval);
    }
};

// Funções utilitárias para formulários
const FormUtils = {

    // Validar formulário
    validateForm: function (form) {
        const inputs = form.querySelectorAll('input[required], select[required], textarea[required]');
        let isValid = true;

        inputs.forEach(input => {
            if (!input.value.trim()) {
                input.classList.add('is-invalid');
                isValid = false;
            } else {
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');
            }
        });

        return isValid;
    },

    // Limpar formulário
    clearForm: function (form) {
        const inputs = form.querySelectorAll('input, select, textarea');
        inputs.forEach(input => {
            if (input.type !== 'hidden') {
                input.value = '';
                input.classList.remove('is-valid', 'is-invalid');
            }
        });
    },

    // Serializar formulário para objeto
    serializeForm: function (form) {
        const formData = new FormData(form);
        const object = {};

        formData.forEach((value, key) => {
            object[key] = value;
        });

        return object;
    }
};

// Expor funções globalmente
window.SislocUtils = SislocUtils;
window.ConsultaFunctions = ConsultaFunctions;
window.AdminFunctions = AdminFunctions;
window.FormUtils = FormUtils;

// Função global para confirmar ações (compatibilidade)
window.confirmarAcao = function (mensagem) {
    return confirm(mensagem || 'Tem certeza que deseja executar esta ação?');
};

// Debug helper (apenas em desenvolvimento)
if (window.location.hostname === 'localhost') {
    window.SislocDebug = {
        config: SislocConfig,
        utils: SislocUtils,
        version: '1.0.0'
    };
    console.log('Sisloc Debug Mode Enabled');
}